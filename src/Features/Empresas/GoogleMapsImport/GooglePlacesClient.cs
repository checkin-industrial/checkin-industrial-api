using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

// Cliente HTTP da Google Places API v1 (Nearby Search por circle).
//
// Auth: header X-Goog-Api-Key + X-Goog-FieldMask (lista de campos a retornar - cobrando
// so o que pede). FieldMask aqui foi escolhido para o tier Pro (basico + endereco +
// telefone + site) mas o tier Essentials cobre o suficiente pra dedup e cadastro
// (apenas places.id + places.displayName + places.location + places.types).
//
// Fail-fast: se ApiKey vazia no options, NearbySearchAsync lanca InvalidOperationException
// com mensagem clara. Nao bloqueia o startup geral - so a feature.
public class GooglePlacesClient : IGooglePlacesClient
{
    private const string FieldMask =
        "places.id,places.displayName,places.formattedAddress,places.location," +
        "places.types,places.nationalPhoneNumber,places.internationalPhoneNumber," +
        "places.websiteUri";

    private readonly HttpClient _http;
    private readonly GoogleMapsOptions _options;
    private readonly ILogger<GooglePlacesClient> _logger;

    public GooglePlacesClient(
        HttpClient http,
        IOptions<GoogleMapsOptions> options,
        ILogger<GooglePlacesClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<GooglePlacesNearbyResponse> NearbySearchAsync(
        double latitude,
        double longitude,
        int radiusMeters,
        IReadOnlyCollection<string> includedTypes,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "GoogleMaps:ApiKey nao configurada. Defina via env var GoogleMaps__ApiKey.");
        }

        // Quando includedTypes for vazio, OMITIR o campo na requisicao
        // (Places API New retorna 400 se enviarmos "includedTypes": []).
        // Sem o campo, a API retorna lugares de qualquer tipo no raio —
        // comportamento do slug "sem-filtro".
        var locationRestriction = new
        {
            circle = new
            {
                center = new { latitude, longitude },
                radius = (double)radiusMeters,
            },
        };
        object body = includedTypes.Count == 0
            ? new { maxResultCount = 20, locationRestriction }
            : new { includedTypes, maxResultCount = 20, locationRestriction };

        var endpointUrl = $"{_options.PlacesBaseUrl.TrimEnd('/')}/places:searchNearby";
        var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Add("X-Goog-Api-Key", _options.ApiKey);
        request.Headers.Add("X-Goog-FieldMask", FieldMask);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        _logger.LogInformation(
            "Google Places NearbySearch iniciando: url={Url} type=NearbySearch lat={Lat} lng={Lng} radius={Radius}m includedTypes=[{Types}]",
            endpointUrl, latitude, longitude, radiusMeters, string.Join(",", includedTypes));

        HttpResponseMessage response;
        string rawJson;
        try
        {
            response = await _http.SendAsync(request, cancellationToken);
            rawJson = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Nao logar ApiKey (vai em header, nao em mensagem) — exception so tem url + tipo.
            _logger.LogError(ex, "Erro ao chamar Google Places NearbySearch em {Url}", endpointUrl);
            throw;
        }

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Google Places retornou {Status}: {Body}",
                (int)response.StatusCode, rawJson);
            throw new HttpRequestException(
                $"Google Places NearbySearch retornou {(int)response.StatusCode}.");
        }

        var places = ParsePlaces(rawJson);
        return new GooglePlacesNearbyResponse(places, rawJson);
    }

    private static IReadOnlyList<GooglePlace> ParsePlaces(string rawJson)
    {
        using var doc = JsonDocument.Parse(rawJson);
        if (!doc.RootElement.TryGetProperty("places", out var placesEl) ||
            placesEl.ValueKind != JsonValueKind.Array)
        {
            return Array.Empty<GooglePlace>();
        }

        var list = new List<GooglePlace>(placesEl.GetArrayLength());
        foreach (var p in placesEl.EnumerateArray())
        {
            var id = p.GetProperty("id").GetString() ?? string.Empty;
            var displayName = p.TryGetProperty("displayName", out var dn) && dn.TryGetProperty("text", out var dnText)
                ? dnText.GetString()
                : null;
            var formattedAddress = p.TryGetProperty("formattedAddress", out var fa) ? fa.GetString() : null;
            var lat = p.TryGetProperty("location", out var loc) && loc.TryGetProperty("latitude", out var latEl)
                ? latEl.GetDouble()
                : 0d;
            var lng = loc.ValueKind == JsonValueKind.Object && loc.TryGetProperty("longitude", out var lngEl)
                ? lngEl.GetDouble()
                : 0d;
            var types = p.TryGetProperty("types", out var typesEl) && typesEl.ValueKind == JsonValueKind.Array
                ? typesEl.EnumerateArray().Select(t => t.GetString() ?? string.Empty).Where(s => s.Length > 0).ToList()
                : new List<string>();
            var nationalPhone = p.TryGetProperty("nationalPhoneNumber", out var np) ? np.GetString() : null;
            var intPhone = p.TryGetProperty("internationalPhoneNumber", out var ip) ? ip.GetString() : null;
            var website = p.TryGetProperty("websiteUri", out var ws) ? ws.GetString() : null;

            list.Add(new GooglePlace(id, displayName, formattedAddress, lat, lng, types, nationalPhone, intPhone, website));
        }
        return list;
    }
}
