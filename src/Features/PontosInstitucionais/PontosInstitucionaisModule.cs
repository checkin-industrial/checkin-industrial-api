namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public static class PontosInstitucionaisModule
{
    public static IServiceCollection AddPontosInstitucionaisFeature(this IServiceCollection services)
    {
        services.AddScoped<IPontoInstitucionalService, PontoInstitucionalService>();
        services.AddScoped<IPontoInstitucionalQuery, PontoInstitucionalQuery>();
        return services;
    }

    public static IEndpointRouteBuilder MapPontosInstitucionaisEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/pontos-institucionais").WithTags("PontosInstitucionais");

        // Reads - publicos, com output cache
        group.MapListPontosInstitucionais().CacheOutput("ReadEndpoint");
        group.MapGetPontoInstitucionalById().CacheOutput("ReadEndpoint");

        // Writes + upload - protegidos por API Key
        group.MapCreatePontoInstitucional().RequireAuthorization();
        group.MapUpdatePontoInstitucional().RequireAuthorization();
        group.MapDeletePontoInstitucional().RequireAuthorization();
        group.MapUploadImagemPontoInstitucional().RequireAuthorization();

        return endpoints;
    }
}
