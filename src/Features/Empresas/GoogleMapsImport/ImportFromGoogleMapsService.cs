using AppTurismoIndustrial.Api.Features.Geocoding;
using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using AppTurismoIndustrial.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

// Orquestra: geocode do CEP -> nearby search -> upsert (criar novas, enriquecer
// existentes via match por GooglePlaceId) -> persiste log. Tudo sincrono no
// primeiro momento (decisao D6 do plano).
public class ImportFromGoogleMapsService : IImportFromGoogleMapsService
{
    private readonly AppDbContext _db;
    private readonly IGeocodingService _geocoding;
    private readonly IGooglePlacesClient _places;
    private readonly GoogleMapsOptions _options;
    private readonly ILogger<ImportFromGoogleMapsService> _logger;

    public ImportFromGoogleMapsService(
        AppDbContext db,
        IGeocodingService geocoding,
        IGooglePlacesClient places,
        IOptions<GoogleMapsOptions> options,
        ILogger<ImportFromGoogleMapsService> logger)
    {
        _db = db;
        _geocoding = geocoding;
        _places = places;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<DTOImportFromGoogleMapsResponse> ImportAsync(
        DTOImportFromGoogleMapsRequest request,
        CancellationToken cancellationToken = default)
    {
        // Cap absoluto vem do options. Validation inline pra evitar request caro.
        if (request.RaioMetros > _options.MaxRaioMetros)
        {
            throw new ValidationException(
                $"Raio maximo permitido e {_options.MaxRaioMetros} metros.");
        }

        var tipoBusca = GooglePlaceTypeMapping.FindBySlug(request.Tipo)
            ?? throw new ValidationException(
                $"Tipo '{request.Tipo}' nao suportado. Tipos suportados: " +
                string.Join(", ", GooglePlaceTypeMapping.SupportedSlugs));

        var geocode = await _geocoding.GeocodeAsync($"CEP {request.Cep}", cancellationToken: cancellationToken)
            ?? throw new ValidationException($"CEP {request.Cep} nao pode ser geocodificado.");

        // Region guard: protege contra requests acidentais fora da regiao prevista.
        if (_options.AllowedRegion is { } region &&
            !region.Contains((double)geocode.Latitude, (double)geocode.Longitude))
        {
            throw new ValidationException(
                $"CEP {request.Cep} (lat={geocode.Latitude}, lng={geocode.Longitude}) " +
                "esta fora da regiao permitida em GoogleMaps:AllowedRegion.");
        }

        var log = new GoogleMapsImportLog
        {
            Cep = request.Cep,
            RaioMetros = request.RaioMetros,
            Tipo = request.Tipo,
            LatitudeOrigem = geocode.Latitude,
            LongitudeOrigem = geocode.Longitude,
        };

        DTOImportFromGoogleMapsResponse response;
        try
        {
            var googleResp = await _places.NearbySearchAsync(
                (double)geocode.Latitude,
                (double)geocode.Longitude,
                request.RaioMetros,
                tipoBusca.GooglePlaceTypes,
                cancellationToken);

            log.ResponseRaw = googleResp.RawJson ?? "{}";

            response = await ProcessPlacesAsync(googleResp.Places, log.Id, request.Cep, cancellationToken);
            response.OperacaoId = log.Id;

            // Reusa os campos historicos do log pra contabilizar candidates
            // (renomear as colunas exigiria migration extra; semanticamente
            // EmpresasCriadas = "candidates criados nesta operacao").
            log.EmpresasCriadas = response.CandidatesCriados;
            log.EmpresasAtualizadas = response.CandidatesAtualizados;
            log.EmpresasIgnoradas = response.CandidatesIgnorados;
        }
        catch (Exception ex)
        {
            log.Erro = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
            _db.GoogleMapsImportLogs.Add(log);
            await _db.SaveChangesAsync(CancellationToken.None);
            throw;
        }

        _db.GoogleMapsImportLogs.Add(log);
        await _db.SaveChangesAsync(cancellationToken);
        return response;
    }

    // Cria/atualiza candidates de triagem (admin promove pra Empresa/Ponto/Telefone
    // depois via endpoints separados). Diferenca chave vs. fluxo antigo: nao cria
    // entidade-fim direto, evita poluir Empresas com dados pre-revisao.
    private async Task<DTOImportFromGoogleMapsResponse> ProcessPlacesAsync(
        IReadOnlyList<GooglePlace> places,
        Guid logId,
        string cepOrigem,
        CancellationToken cancellationToken)
    {
        var placeIds = places.Select(p => p.Id).ToList();
        var existentes = await _db.GoogleMapsImportCandidates
            .Where(c => placeIds.Contains(c.GooglePlaceId))
            .ToDictionaryAsync(c => c.GooglePlaceId, cancellationToken);

        var itens = new List<DTOImportResultItem>(places.Count);
        int criados = 0, atualizados = 0, ignorados = 0;

        foreach (var p in places)
        {
            if (existentes.TryGetValue(p.Id, out var existente))
            {
                var enriquecido = EnriquecerCandidate(existente, p);
                if (enriquecido)
                {
                    atualizados++;
                    itens.Add(new DTOImportResultItem
                    {
                        GooglePlaceId = p.Id,
                        Nome = p.DisplayName ?? string.Empty,
                        Acao = "atualizado",
                        CandidateId = existente.Id,
                    });
                }
                else
                {
                    ignorados++;
                    itens.Add(new DTOImportResultItem
                    {
                        GooglePlaceId = p.Id,
                        Nome = p.DisplayName ?? string.Empty,
                        Acao = "ignorado",
                        CandidateId = existente.Id,
                        Motivo = "Candidate ja existia, sem campos novos para enriquecer.",
                    });
                }
            }
            else
            {
                var novo = CriarCandidateDeGoogle(p, logId, cepOrigem);
                _db.GoogleMapsImportCandidates.Add(novo);
                criados++;
                itens.Add(new DTOImportResultItem
                {
                    GooglePlaceId = p.Id,
                    Nome = p.DisplayName ?? string.Empty,
                    Acao = "criado",
                    CandidateId = novo.Id,
                });
            }
        }

        return new DTOImportFromGoogleMapsResponse
        {
            Encontrados = places.Count,
            CandidatesCriados = criados,
            CandidatesAtualizados = atualizados,
            CandidatesIgnorados = ignorados,
            Itens = itens,
        };
    }

    private static GoogleMapsImportCandidate CriarCandidateDeGoogle(
        GooglePlace p, Guid logId, string cepOrigem)
    {
        var nome = !string.IsNullOrWhiteSpace(p.DisplayName) ? p.DisplayName! : "(sem nome)";
        var typesJson = System.Text.Json.JsonSerializer.Serialize(p.Types);
        return new GoogleMapsImportCandidate
        {
            GoogleMapsImportLogId = logId,
            GooglePlaceId = p.Id,
            Nome = Truncar(nome, 200),
            FormattedAddress = Truncar(p.FormattedAddress ?? string.Empty, 500),
            Latitude = (decimal)p.Latitude,
            Longitude = (decimal)p.Longitude,
            Telefone = p.NationalPhoneNumber ?? p.InternationalPhoneNumber,
            TypesJson = typesJson,
            CepOrigem = string.IsNullOrWhiteSpace(cepOrigem) ? null : cepOrigem,
        };
    }

    // Enriquecimento conservador: nao sobrescreve dado preenchido com algo do Google.
    // Apenas preenche campos vazios + sempre atualiza types/coords (info volatil).
    private static bool EnriquecerCandidate(GoogleMapsImportCandidate c, GooglePlace p)
    {
        var mudou = false;

        if (string.IsNullOrWhiteSpace(c.Telefone) && !string.IsNullOrWhiteSpace(p.NationalPhoneNumber))
        {
            c.Telefone = p.NationalPhoneNumber;
            mudou = true;
        }

        if (string.IsNullOrWhiteSpace(c.FormattedAddress) && !string.IsNullOrWhiteSpace(p.FormattedAddress))
        {
            c.FormattedAddress = Truncar(p.FormattedAddress!, 500);
            mudou = true;
        }

        var typesJsonAtual = System.Text.Json.JsonSerializer.Serialize(p.Types);
        if (!string.Equals(c.TypesJson, typesJsonAtual, StringComparison.Ordinal))
        {
            c.TypesJson = typesJsonAtual;
            mudou = true;
        }

        return mudou;
    }

    private static string Truncar(string s, int max) => s.Length <= max ? s : s[..max];
}
