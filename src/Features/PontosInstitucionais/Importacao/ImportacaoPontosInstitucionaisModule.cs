namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais.Importacao;

public static class ImportacaoPontosInstitucionaisModule
{
    public static IEndpointRouteBuilder MapImportacaoPontosInstitucionaisEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Mesmo grupo /api/import usado por ImportacaoEmpresasModule (rotas distintas).
        var group = endpoints.MapGroup("/api/import").WithTags("ImportacaoPontosInstitucionais");

        // Admin-only (mesma logica de Importacao Empresas)
        group.MapImportPontosInstitucionais().RequireAuthorization();
        group.MapExportPontosInstitucionaisCsv().RequireAuthorization();
        group.MapExportPontosInstitucionaisCsvAnsi().RequireAuthorization();

        return endpoints;
    }
}
