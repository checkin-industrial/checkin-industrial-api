using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Features.Empresas.Importacao;

public static class ExportEmpresasCsvAnsi
{
    public static RouteHandlerBuilder MapExportEmpresasCsvAnsi(this RouteGroupBuilder group)
    {
        return group.MapGet("/empresas/exportar-ansi", Handle)
            .WithName(nameof(ExportEmpresasCsvAnsi))
            .Produces(StatusCodes.Status200OK, contentType: "text/csv");
    }

    /// <summary>
    /// Exporta o cadastro com encoding Windows-1252 (codepage 1252),
    /// util para abertura direta em versoes legadas do Excel no Windows.
    /// </summary>
    private static async Task<FileContentHttpResult> Handle(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var empresas = await context.Empresas
            .AsNoTracking()
            .OrderBy(e => e.NomeFantasia)
            .ToListAsync(cancellationToken);

        var encoding = Encoding.GetEncoding(1252);
        var conteudo = await EmpresaCsvFormatter.GerarCsvAsync(
            empresas,
            encoding,
            cancellationToken);

        var fileName = $"cadastro-empresas-atual-ansi-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        return TypedResults.File(conteudo, "text/csv; charset=windows-1252", fileName);
    }
}
