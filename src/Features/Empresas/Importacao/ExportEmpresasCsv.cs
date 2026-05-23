using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Features.Empresas.Importacao;

public static class ExportEmpresasCsv
{
    public static RouteHandlerBuilder MapExportEmpresasCsv(this RouteGroupBuilder group)
    {
        return group.MapGet("/empresas/exportar", Handle)
            .WithName(nameof(ExportEmpresasCsv))
            .Produces(StatusCodes.Status200OK, contentType: "text/csv");
    }

    private static async Task<FileContentHttpResult> Handle(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var empresas = await context.Empresas
            .AsNoTracking()
            .OrderBy(e => e.NomeFantasia)
            .ToListAsync(cancellationToken);

        var conteudo = await EmpresaCsvFormatter.GerarCsvAsync(
            empresas,
            new UTF8Encoding(true),
            cancellationToken);

        var fileName = $"cadastro-empresas-atual-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";
        return TypedResults.File(conteudo, "text/csv; charset=utf-8", fileName);
    }
}
