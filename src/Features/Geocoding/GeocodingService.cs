using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace AppTurismoIndustrial.Api.Features.Geocoding;

/// <summary>
/// Implementação do serviço de geocodificação com cache local.
/// Suporta múltiplos provedores de geocodificação (Google Maps, OpenStreetMap, etc).
/// </summary>
public class GeocodingService : IGeocodingService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<GeocodingService> _logger;
    private readonly IGeocodingProvider _provider;
    private readonly IViaCepClient _viaCep;

    // Cacheia por até 30 dias para evitar chamadas repetidas
    private readonly MemoryCacheEntryOptions _cacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30),
        SlidingExpiration = TimeSpan.FromDays(7)
    };

    public GeocodingService(
        IMemoryCache cache,
        ILogger<GeocodingService> logger,
        IGeocodingProvider provider,
        IViaCepClient viaCep)
    {
        _cache = cache;
        _logger = logger;
        _provider = provider;
        _viaCep = viaCep;
    }

    public async Task<GeocodeResult?> GeocodeAsync(
        string endereco,
        string? cidade = null,
        string? estado = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(endereco))
        {
            _logger.LogWarning("Tentativa de geocodificar endereço vazio");
            return null;
        }

        // Normaliza chave de cache
        var cacheKey = NormalizeCacheKey(endereco, cidade, estado);

        // Verifica cache
        if (_cache.TryGetValue(cacheKey, out GeocodeResult? resultado))
        {
            _logger.LogInformation("Geocodificação obtida do cache: {Endereco}", endereco);
            return resultado;
        }

        try
        {
            // Estrategia em multi-passo pra CEP brasileiro:
            // 1. ViaCEP resolve CEP -> endereco estruturado (logradouro, localidade, uf).
            // 2. Nominatim geocodifica a query mais especifica que ele consegue.
            //
            // Monta lista de queries candidatas em ordem da mais especifica pra mais
            // generica. Cada candidata e tentada no provedor; primeira que retornar
            // result nao-nulo vence. Padroes observados no OSM Brasil:
            //   - Logradouro + Cidade: tipicamente acerta (logradouros sao bem mapeados)
            //   - Bairro + Cidade: tipicamente VAZIO (bairros raramente mapeados como
            //     entidades pesquisaveis no OSM Brasil — proposital pulado abaixo)
            //   - So Cidade: fallback que sempre acerta para municipios reais
            // Sem isso, CEPs do interior cairiam em municipios errados (ex: CEP 18681420
            // / Lencois Paulista era geocodificado em Araraquara, 50 km de erro).
            var candidates = new List<(string Endereco, string? Cidade, string? Estado)>();

            if (ViaCepClient.CepRegex.IsMatch(endereco))
            {
                _logger.LogInformation("Input parece CEP — consultando ViaCEP: {Endereco}", endereco);
                var viaCepResult = await _viaCep.ResolveAsync(endereco, cancellationToken);
                if (viaCepResult is not null)
                {
                    _logger.LogInformation(
                        "ViaCEP resolveu {Endereco} -> {Logradouro}, {Localidade}/{Uf}",
                        endereco, viaCepResult.Logradouro, viaCepResult.Localidade, viaCepResult.Uf);

                    // Candidata 1: logradouro + cidade + uf (melhor precisao, ~rua exata).
                    if (!string.IsNullOrWhiteSpace(viaCepResult.Logradouro))
                    {
                        candidates.Add((viaCepResult.Logradouro, viaCepResult.Localidade, viaCepResult.Uf));
                    }
                    // Candidata 2: so cidade + uf (precisao "centro da cidade", suficiente
                    // para o uso de import-por-raio onde o centro da busca e a cidade).
                    candidates.Add((viaCepResult.Localidade, viaCepResult.Localidade, viaCepResult.Uf));
                }
                else
                {
                    _logger.LogInformation("ViaCEP nao resolveu — fallback pra query direta no Nominatim");
                }
            }

            // Sempre inclui a query original como ultimo fallback (cobre tanto enderecos
            // textuais que pulam o ViaCEP quanto CEPs que o ViaCEP rejeitou).
            candidates.Add((endereco, cidade, estado));

            GeocodeResult? resultado_obtido = null;
            foreach (var (candEndereco, candCidade, candEstado) in candidates)
            {
                _logger.LogInformation(
                    "Geocodificando via {Provider}: {Endereco} ({Cidade}/{Estado})",
                    _provider.ProviderName, candEndereco, candCidade, candEstado);

                resultado_obtido = await _provider.GeocodeAsync(candEndereco, candCidade, candEstado, cancellationToken);
                if (resultado_obtido != null) break;
            }

            if (resultado_obtido != null)
            {
                // Armazena resultado no cache
                _cache.Set(cacheKey, resultado_obtido, _cacheOptions);
                _logger.LogInformation(
                    "Geocodificação realizada: {Endereco} -> ({Lat}, {Lon})",
                    endereco,
                    resultado_obtido.Latitude,
                    resultado_obtido.Longitude);

                return resultado_obtido;
            }
            else
            {
                _logger.LogWarning("Geocodificação não encontrada: {Endereco}", endereco);
                
                // Armazena falha em cache por 7 dias para evitar tentativas repetidas
                _cache.Set(cacheKey, (GeocodeResult?)null, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
                });

                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao geocodificar endereço: {Endereco}", endereco);
            return null;
        }
    }

    public void ClearCache()
    {
        // IMemoryCache não oferece método para limpar tudo, apenas entradas individuais
        // A solução é usar um IMemoryCache com chaves conhecidas ou implementar custom cache
        _logger.LogInformation("Cache de geocodificação limpo (operação - informativa)");
    }

    public int GetCacheSize()
    {
        // IMemoryCache não expõe tamanho direto, seria necessário implementar custom cache
        // Para produção, considere usar Redis ou implementar IMemoryCache customizado
        return 0;
    }

    /// <summary>
    /// Normaliza a chave de cache a partir dos parâmetros de entrada.
    /// </summary>
    private string NormalizeCacheKey(string endereco, string? cidade, string? estado)
    {
        var normalizacao = new System.Text.StringBuilder("GEOCODE_");
        normalizacao.Append(endereco.ToLowerInvariant().Trim().GetHashCode());

        if (!string.IsNullOrWhiteSpace(cidade))
        {
            normalizacao.Append("_");
            normalizacao.Append(cidade.ToLowerInvariant().Trim().GetHashCode());
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            normalizacao.Append("_");
            normalizacao.Append(estado.ToLowerInvariant().Trim().GetHashCode());
        }

        return normalizacao.ToString();
    }
}

/// <summary>
/// Interface para provedores de geocodificação (Google Maps, OpenStreetMap, etc).
/// Permite trocar implementação sem alterar o serviço principal.
/// </summary>
public interface IGeocodingProvider
{
    /// <summary>
    /// Geocodifica um endereço usando o provedor específico.
    /// </summary>
    Task<GeocodeResult?> GeocodeAsync(
        string endereco,
        string? cidade = null,
        string? estado = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Nome do provedor para logging e rastreabilidade.
    /// </summary>
    string ProviderName { get; }
}

/// <summary>
/// Implementação stub de provedor de geocodificação.
/// Em produção, integrar com OpenStreetMap, Google Maps, etc.
/// </summary>
public class StubGeocodingProvider : IGeocodingProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<StubGeocodingProvider> _logger;

    public StubGeocodingProvider(IHttpClientFactory httpClientFactory, ILogger<StubGeocodingProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public string ProviderName => "OpenStreetMap-Nominatim";

    public Task<GeocodeResult?> GeocodeAsync(
        string endereco,
        string? cidade = null,
        string? estado = null,
        CancellationToken cancellationToken = default)
    {
        return GeocodeFromNominatimAsync(endereco, cidade, estado, cancellationToken);
    }

    private async Task<GeocodeResult?> GeocodeFromNominatimAsync(
        string endereco,
        string? cidade,
        string? estado,
        CancellationToken cancellationToken)
    {
        var queryParts = new List<string> { endereco };

        if (!string.IsNullOrWhiteSpace(cidade))
        {
            queryParts.Add(cidade);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            queryParts.Add(estado);
        }

        queryParts.Add("Brasil");
        var query = string.Join(", ", queryParts.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));
        var url = $"https://nominatim.openstreetmap.org/search?format=json&limit=1&q={Uri.EscapeDataString(query)}";

        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("AppTurismoIndustrial", "1.0"));
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("(+https://localhost)"));
            request.Headers.AcceptLanguage.ParseAdd("pt-BR,pt;q=0.9");

            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Nominatim retornou status {StatusCode} para query {Query}", response.StatusCode, query);
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            var results = JsonSerializer.Deserialize<List<NominatimResult>>(responseBody);
            var first = results?.FirstOrDefault();
            if (first is null)
            {
                return null;
            }

            if (!decimal.TryParse(first.lat, NumberStyles.Any, CultureInfo.InvariantCulture, out var latitude)
                || !decimal.TryParse(first.lon, NumberStyles.Any, CultureInfo.InvariantCulture, out var longitude))
            {
                return null;
            }

            return new GeocodeResult
            {
                Latitude = Math.Round(latitude, 6),
                Longitude = Math.Round(longitude, 6),
                Accuracy = string.IsNullOrWhiteSpace(first.type) ? "aproximado" : first.type,
                Provider = ProviderName,
                ObtainedAt = DateTime.UtcNow,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar Nominatim para o endereço {Endereco}", endereco);
            return null;
        }
    }

    private sealed class NominatimResult
    {
        public string lat { get; set; } = string.Empty;
        public string lon { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
    }
}
