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

            response = await ProcessPlacesAsync(googleResp.Places, tipoBusca, cancellationToken);
            response.OperacaoId = log.Id;

            log.EmpresasCriadas = response.Criados;
            log.EmpresasAtualizadas = response.Atualizados;
            log.EmpresasIgnoradas = response.Ignorados;
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

    private async Task<DTOImportFromGoogleMapsResponse> ProcessPlacesAsync(
        IReadOnlyList<GooglePlace> places,
        GooglePlaceTypeMapping.TipoBusca tipoBusca,
        CancellationToken cancellationToken)
    {
        var placeIds = places.Select(p => p.Id).ToList();
        var existentes = await _db.Empresas
            .Where(e => e.GooglePlaceId != null && placeIds.Contains(e.GooglePlaceId))
            .ToDictionaryAsync(e => e.GooglePlaceId!, cancellationToken);

        var itens = new List<DTOImportResultItem>(places.Count);
        int criados = 0, atualizados = 0, ignorados = 0;

        foreach (var p in places)
        {
            if (existentes.TryGetValue(p.Id, out var existente))
            {
                var enriquecido = EnriquecerEmpresa(existente, p);
                if (enriquecido)
                {
                    atualizados++;
                    itens.Add(new DTOImportResultItem
                    {
                        GooglePlaceId = p.Id,
                        Nome = p.DisplayName ?? string.Empty,
                        Acao = "atualizado",
                        EmpresaId = existente.Id,
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
                        EmpresaId = existente.Id,
                        Motivo = "Sem campos novos para enriquecer.",
                    });
                }
            }
            else
            {
                var novo = CriarEmpresaDeGoogle(p, tipoBusca);
                _db.Empresas.Add(novo);
                criados++;
                itens.Add(new DTOImportResultItem
                {
                    GooglePlaceId = p.Id,
                    Nome = p.DisplayName ?? string.Empty,
                    Acao = "criado",
                    EmpresaId = novo.Id,
                });
            }
        }

        return new DTOImportFromGoogleMapsResponse
        {
            Encontrados = places.Count,
            Criados = criados,
            Atualizados = atualizados,
            Ignorados = ignorados,
            Itens = itens,
        };
    }

    private static Empresa CriarEmpresaDeGoogle(GooglePlace p, GooglePlaceTypeMapping.TipoBusca tipoBusca)
    {
        var nome = !string.IsNullOrWhiteSpace(p.DisplayName) ? p.DisplayName! : "(sem nome - revisar)";
        return new Empresa
        {
            // Cnpj null - admin preenche antes de reativar
            RazaoSocial = Truncar(nome, 200),
            NomeFantasia = Truncar(nome, 200),
            // CNAE generico - revisar manualmente
            CnaePrincipal = "0000000",
            DescricaoCnae = Truncar(string.Join(",", p.Types), 300),
            Setor = tipoBusca.SetorDefault,
            Porte = PorteEmpresa.Me,
            Endereco = Truncar(p.FormattedAddress ?? "(sem endereco)", 300),
            Telefone = p.NationalPhoneNumber ?? p.InternationalPhoneNumber,
            // CEP nao vem do Places diretamente, deixa null
            Municipio = "Importado",
            MatrizOuFilial = MatrizOuFilialEmpresa.Matriz,
            Latitude = (decimal)p.Latitude,
            Longitude = (decimal)p.Longitude,
            SituacaoCadastral = SituacaoCadastral.Ativa,
            GooglePlaceId = p.Id,
            Ativo = false,  // ⭐ fluxo de revisao
        };
    }

    // Enriquecimento conservador: nunca sobrescreve dado preenchido com algo do Google.
    // Apenas preenche campos vazios (telefone, endereco mais rico, coordenadas mais precisas).
    private static bool EnriquecerEmpresa(Empresa empresa, GooglePlace p)
    {
        var mudou = false;

        if (string.IsNullOrWhiteSpace(empresa.Telefone) && !string.IsNullOrWhiteSpace(p.NationalPhoneNumber))
        {
            empresa.Telefone = p.NationalPhoneNumber;
            mudou = true;
        }

        if ((string.IsNullOrWhiteSpace(empresa.Endereco) || empresa.Endereco.Equals("(sem endereco)", StringComparison.Ordinal))
            && !string.IsNullOrWhiteSpace(p.FormattedAddress))
        {
            empresa.Endereco = Truncar(p.FormattedAddress!, 300);
            mudou = true;
        }

        return mudou;
    }

    private static string Truncar(string s, int max) => s.Length <= max ? s : s[..max];
}
