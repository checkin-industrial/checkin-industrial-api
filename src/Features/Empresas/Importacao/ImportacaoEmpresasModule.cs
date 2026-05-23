namespace AppTurismoIndustrial.Api.Features.Empresas.Importacao;

public static class ImportacaoEmpresasModule
{
    public static IServiceCollection AddImportacaoEmpresasFeature(this IServiceCollection services)
    {
        services.AddScoped<IEmpresaImportParser, CsvEmpresaParser>();
        services.AddScoped<IImportacaoEmpresasService, ImportacaoEmpresasService>();
        return services;
    }

    public static IEndpointRouteBuilder MapImportacaoEmpresasEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/import").WithTags("ImportacaoEmpresas");
        group.MapImportEmpresas();
        group.MapExportEmpresasCsv();
        group.MapExportEmpresasCsvAnsi();
        return endpoints;
    }
}
