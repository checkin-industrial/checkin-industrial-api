namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais.Importacao;

public static class ImportacaoPontosInstitucionaisModule
{
    public static IEndpointRouteBuilder MapImportacaoPontosInstitucionaisEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Mesmo grupo /api/import usado por ImportacaoEmpresasModule (rotas distintas).
        var group = endpoints.MapGroup("/api/import").WithTags("ImportacaoPontosInstitucionais");
        group.MapImportPontosInstitucionais();
        group.MapExportPontosInstitucionaisCsv();
        group.MapExportPontosInstitucionaisCsvAnsi();
        return endpoints;
    }
}
