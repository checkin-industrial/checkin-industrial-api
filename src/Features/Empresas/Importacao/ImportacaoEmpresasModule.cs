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

        // Tudo aqui e admin-only (import + exports expoem o cadastro completo).
        group.MapImportEmpresas().RequireAuthorization();
        group.MapExportEmpresasCsv().RequireAuthorization();
        group.MapExportEmpresasCsvAnsi().RequireAuthorization();

        return endpoints;
    }
}
