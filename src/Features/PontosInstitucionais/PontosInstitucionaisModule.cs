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
        group.MapListPontosInstitucionais();
        group.MapGetPontoInstitucionalById();
        group.MapCreatePontoInstitucional();
        group.MapUpdatePontoInstitucional();
        group.MapDeletePontoInstitucional();
        group.MapUploadImagemPontoInstitucional();
        return endpoints;
    }
}
