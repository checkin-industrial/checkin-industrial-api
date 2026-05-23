namespace AppTurismoIndustrial.Api.Features.Geocoding;

public static class GeocodingModule
{
    public static IServiceCollection AddGeocodingFeature(this IServiceCollection services)
    {
        services.AddScoped<IGeocodingProvider, StubGeocodingProvider>();
        services.AddScoped<IGeocodingService, GeocodingService>();
        return services;
    }

    public static IEndpointRouteBuilder MapGeocodingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Mantida a rota historica /api/empresas/geocode para nao quebrar consumidores existentes.
        // Protegida por API Key (uso admin no painel + consome API externa Nominatim).
        endpoints.MapGeocodeAddress().RequireAuthorization();
        return endpoints;
    }
}
