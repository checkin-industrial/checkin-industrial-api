using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais.Importacao;

public static class ExportPontosInstitucionaisCsvAnsi
{
    public static RouteHandlerBuilder MapExportPontosInstitucionaisCsvAnsi(this RouteGroupBuilder group)
    {
        return group.MapGet("/pontos-institucionais/exportar-ansi", Handle)
            .WithName(nameof(ExportPontosInstitucionaisCsvAnsi))
            .Produces(StatusCodes.Status200OK, contentType: "text/csv");
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

        var conteudo = await PontoInstitucionalCsvFormatter.GerarCsvAsync(pontos, Encoding.GetEncoding(1252), cancellationToken);
        var fileName = $"cadastro-pontos-institucionais-ansi-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return TypedResults.File(conteudo, "text/csv; charset=windows-1252", fileName);
    }
}
