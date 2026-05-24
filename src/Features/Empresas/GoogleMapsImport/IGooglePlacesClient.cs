namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

// Abstracao em torno da Google Places API v1 (Nearby Search). Definida como interface
// pra permitir mock em testes (sem tocar a API real - chamadas custam dinheiro).
public interface IGooglePlacesClient
{
    Task<GooglePlacesNearbyResponse> NearbySearchAsync(
        double latitude,
        double longitude,
        int radiusMeters,
        IReadOnlyCollection<string> includedTypes,
        CancellationToken cancellationToken = default);
}

// DTOs intencionalmente "flat" - so os campos que consumimos. Schema completo do
// Google Places v1 esta em https://developers.google.com/maps/documentation/places/web-service/nearby-search
public record GooglePlacesNearbyResponse(IReadOnlyList<GooglePlace> Places, string? RawJson);

public record GooglePlace(
    string Id,
    string? DisplayName,
    string? FormattedAddress,
    double Latitude,
    double Longitude,
    IReadOnlyList<string> Types,
    string? NationalPhoneNumber,
    string? InternationalPhoneNumber,
    string? WebsiteUri);
