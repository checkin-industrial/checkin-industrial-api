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
        group.MapListEmpresas();
        group.MapFilterEmpresas();
        group.MapGetEmpresaNeighbors();
        group.MapGetEmpresaById();
        group.MapCreateEmpresa();
        group.MapUpdateEmpresa();
        group.MapDeleteEmpresa();
        return endpoints;
    }
}
