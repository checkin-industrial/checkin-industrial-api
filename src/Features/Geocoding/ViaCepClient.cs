using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AppTurismoIndustrial.Api.Features.Geocoding;

/// <summary>
/// Resolve CEPs brasileiros via ViaCEP (https://viacep.com.br) — API pública,
/// gratuita, sem autenticação e mantida pelos próprios Correios indiretamente.
///
/// O Nominatim/OSM tem cobertura ruim para CEPs brasileiros (testes mostraram
/// que CEP 18681-420 era resolvido com 50 km de erro para Araraquara em vez
/// de Lençóis Paulista). O ViaCEP devolve o endereço estruturado (logradouro,
/// bairro, localidade, UF) que aí pode ser geocodificado com precisão pelo
/// Nominatim com a string completa.
///
/// Fluxo no GeocodingService:
///   1. Se o input casa CEP_REGEX, chama ViaCepClient.ResolveAsync.
///   2. Sucesso: monta endereço completo "{logradouro}, {bairro}, {localidade}, {uf}, Brasil"
///      e geocodifica isso via Nominatim — fica preciso.
///   3. Falha (CEP inválido, network error): fallback para query original.
/// </summary>
public interface IViaCepClient
{
    Task<ViaCepAddress?> ResolveAsync(string cep, CancellationToken cancellationToken = default);
}

public sealed record ViaCepAddress(
    string Cep,
    string Logradouro,
    string Bairro,
    string Localidade,
    string Uf);

public sealed partial class ViaCepClient : IViaCepClient
{
    // CEP brasileiro: 8 dígitos, com ou sem hífen entre o 5º e o 6º dígito.
    // Public porque GeocodingService usa pra detectar se deve invocar ViaCEP.
    public static readonly Regex CepRegex = MyRegex();

    private const string BaseUrl = "https://viacep.com.br/ws";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ViaCepClient> _logger;

    public ViaCepClient(IHttpClientFactory httpClientFactory, ILogger<ViaCepClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<ViaCepAddress?> ResolveAsync(string cep, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cep))
        {
            return null;
        }

        // Normaliza: só dígitos. CepRegex já garantiu formato no caller, mas defesa
        // adicional protege contra inputs como "CEP 18681420" (string composta).
        var digits = new string(cep.Where(char.IsDigit).ToArray());
        if (digits.Length != 8)
        {
            return null;
        }

        var url = $"{BaseUrl}/{digits}/json/";

        try
        {
            var client = _httpClientFactory.CreateClient();
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.Add(new ProductInfoHeaderValue("AppTurismoIndustrial", "1.0"));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            using var response = await client.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ViaCEP retornou status {Status} para CEP {Cep}", (int)response.StatusCode, digits);
                return null;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var raw = JsonSerializer.Deserialize<ViaCepRawResponse>(body);

            // ViaCEP retorna 200 com `{ "erro": "true" }` (ou true bool) em CEP inválido.
            if (raw is null || raw.Erro is { } erro && (erro.ValueKind == JsonValueKind.True ||
                (erro.ValueKind == JsonValueKind.String && erro.GetString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)))
            {
                _logger.LogInformation("ViaCEP marcou CEP {Cep} como inválido (erro=true)", digits);
                return null;
            }

            if (string.IsNullOrWhiteSpace(raw.Localidade) || string.IsNullOrWhiteSpace(raw.Uf))
            {
                _logger.LogInformation("ViaCEP retornou CEP {Cep} sem localidade/uf — descartando", digits);
                return null;
            }

            return new ViaCepAddress(
                Cep: raw.Cep ?? digits,
                Logradouro: raw.Logradouro ?? string.Empty,
                Bairro: raw.Bairro ?? string.Empty,
                Localidade: raw.Localidade,
                Uf: raw.Uf);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Erro ao consultar ViaCEP para CEP {Cep}", digits);
            return null;
        }
    }

    // Aceita: "18681420", "18681-420", "CEP 18681420", "  CEP  18681-420  ".
    [GeneratedRegex(@"^\s*(CEP\s+)?\d{5}-?\d{3}\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex MyRegex();

    private sealed class ViaCepRawResponse
    {
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Localidade { get; set; }
        public string? Uf { get; set; }
        // ViaCEP pode retornar erro como string "true" ou bool — manteve JsonElement
        // para tolerar ambas.
        public JsonElement? Erro { get; set; }
    }
}
