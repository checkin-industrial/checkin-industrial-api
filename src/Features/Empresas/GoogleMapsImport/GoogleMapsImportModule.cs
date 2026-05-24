namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

public static class GoogleMapsImportModule
{
    public static IServiceCollection AddGoogleMapsImportFeature(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<GoogleMapsOptions>(configuration.GetSection(GoogleMapsOptions.SectionName));
        services.AddHttpClient<IGooglePlacesClient, GooglePlacesClient>(http =>
        {
            http.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddScoped<IImportFromGoogleMapsService, ImportFromGoogleMapsService>();
        return services;
    }

    public static IEndpointRouteBuilder MapGoogleMapsImportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/empresas/import")
            .WithTags("EmpresasImport");

        group.MapImportFromGoogleMaps().RequireAuthorization();
        return endpoints;
    }
}
