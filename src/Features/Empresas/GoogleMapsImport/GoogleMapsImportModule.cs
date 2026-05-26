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
        services.AddScoped<IImportCandidateService, ImportCandidateService>();
        return services;
    }

    public static IEndpointRouteBuilder MapGoogleMapsImportEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Endpoint historico de disparar import via Google Maps. Agora cria candidates
        // (triagem) em vez de Empresas direto.
        var importGroup = endpoints.MapGroup("/api/empresas/import")
            .WithTags("EmpresasImport");
        importGroup.MapImportFromGoogleMaps().RequireAuthorization();

        // Endpoints da triagem (admin promove/rejeita candidates por destino).
        var candidatesGroup = endpoints.MapGroup("/api/import/candidates")
            .WithTags("ImportCandidates");
        candidatesGroup.MapListImportCandidates().RequireAuthorization();
        candidatesGroup.MapGetImportCandidateById().RequireAuthorization();
        candidatesGroup.MapPromoteCandidateToEmpresa().RequireAuthorization();
        candidatesGroup.MapPromoteCandidateToPonto().RequireAuthorization();
        candidatesGroup.MapPromoteCandidateToTelefone().RequireAuthorization();
        candidatesGroup.MapRejectImportCandidate().RequireAuthorization();

        return endpoints;
    }
}
