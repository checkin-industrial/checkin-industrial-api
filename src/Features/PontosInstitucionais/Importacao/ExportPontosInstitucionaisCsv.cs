using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais.Importacao;

public static class ExportPontosInstitucionaisCsv
{
    public static RouteGroupBuilder MapExportPontosInstitucionaisCsv(this RouteGroupBuilder group)
    {
        group.MapGet("/pontos-institucionais/exportar", Handle)
            .WithName(nameof(ExportPontosInstitucionaisCsv))
            .Produces(StatusCodes.Status200OK, contentType: "text/csv");
        return group;
    }

    private static async Task<FileContentHttpResult> Handle(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var pontos = await context.PontosInstitucionais
            .AsNoTracking()
            .OrderBy(p => p.OrdemExibicao ?? 0)
            .ThenBy(p => p.Nome)
            .ToListAsync(cancellationToken);

        var conteudo = await PontoInstitucionalCsvFormatter.GerarCsvAsync(pontos, new UTF8Encoding(true), cancellationToken);
        var fileName = $"cadastro-pontos-institucionais-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return TypedResults.File(conteudo, "text/csv; charset=utf-8", fileName);
    }
}
