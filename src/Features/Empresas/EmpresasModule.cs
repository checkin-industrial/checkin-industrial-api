namespace AppTurismoIndustrial.Api.Features.Empresas;

public static class EmpresasModule
{
    public static IServiceCollection AddEmpresasFeature(this IServiceCollection services)
    {
        services.AddScoped<IEmpresaService, EmpresaService>();
        services.AddScoped<IEmpresaMapService, EmpresaMapService>();
        services.AddScoped<IEmpresaFilterService, EmpresaFilterService>();
        services.AddScoped<IEmpresaNeighborhoodService, EmpresaNeighborhoodService>();
        services.AddScoped<IEmpresaFilterQuery, EmpresaFilterQuery>();
        return services;
    }

    public static IEndpointRouteBuilder MapEmpresasEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/empresas").WithTags("Empresas");

        // Reads - publicos (anonimo OK), com output cache
        group.MapListEmpresas().CacheOutput("ReadEndpoint");
        group.MapFilterEmpresas().CacheOutput("ReadEndpoint");
        group.MapGetEmpresaNeighbors().CacheOutput("ReadEndpoint");
        group.MapGetEmpresaById().CacheOutput("ReadEndpoint");

        // Writes - protegidos por API Key
        group.MapCreateEmpresa().RequireAuthorization();
        group.MapUpdateEmpresa().RequireAuthorization();
        group.MapDeleteEmpresa().RequireAuthorization();

        return endpoints;
    }
}
