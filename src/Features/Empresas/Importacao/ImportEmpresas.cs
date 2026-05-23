using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas.Importacao;

public static class ImportEmpresas
{
    private const long MaxFileSize = 100 * 1024 * 1024; // 100 MB

    public static RouteGroupBuilder MapImportEmpresas(this RouteGroupBuilder group)
    {
        group.MapPost("/empresas", Handle)
            .WithName(nameof(ImportEmpresas))
            .DisableAntiforgery();
        return group;
    }

    /// <summary>
    /// Importa empresas a partir de um arquivo CSV ou JSON via multipart/form-data.
    /// Status retornados: 200 (concluido), 202 (assincrono), 400, 415 (formato), 500.
    /// </summary>
    private static async Task<IResult> Handle(
        IFormFile file,
        IImportacaoEmpresasService importacaoService,
        ILogger<DTOEmpresaCriar> logger,
        CancellationToken cancellationToken)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var validacao = ValidarArquivo(file);
        if (!validacao.Valido)
        {
            logger.LogWarning("Arquivo rejeitado: {Motivo}", validacao.Motivo);
            return TypedResults.BadRequest(new { erro = validacao.Motivo });
        }

        var formato = IdentificarFormato(file.FileName, file.ContentType);
        if (string.IsNullOrEmpty(formato))
        {
            logger.LogWarning("Formato nao reconhecido: {FileName}", file.FileName);
            return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }

        try
        {
            logger.LogInformation("Iniciando importacao: {FileName} ({Tamanho} bytes, Formato: {Formato})",
                file.FileName, file.Length, formato);

            await using var stream = file.OpenReadStream();
            var resultado = await importacaoService.ImportarAsync(stream, formato, cancellationToken);

            var statusCode = resultado.Status switch
            {
                "Completed" => StatusCodes.Status200OK,
                "CompletedWithErrors" => StatusCodes.Status200OK,
                "InProgress" => StatusCodes.Status202Accepted,
                "Pending" => StatusCodes.Status202Accepted,
                _ => StatusCodes.Status500InternalServerError
            };

            logger.LogInformation(
                "Importacao concluda: {Total} registros, {Inseridos} inseridos, {Atualizados} atualizados, {Ignorados} ignorados, {Erros} erros",
                resultado.TotalRecords, resultado.Inserted, resultado.Updated, resultado.Skipped, resultado.Errors.Count);

            return Results.Json(resultado, statusCode: statusCode);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Importacao cancelada pelo usuario");
            return TypedResults.BadRequest(new { erro = "Importacao cancelada." });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro durante importacao de empresas");
            return Results.Problem(
                detail: "Erro ao processar importacao. Tente novamente mais tarde.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    private static string? IdentificarFormato(string nomeArquivo, string contentType)
    {
        var extensao = Path.GetExtension(nomeArquivo).ToLowerInvariant();
        if (extensao == ".csv" || contentType.Contains("csv"))
        {
            return "CSV";
        }
        if (extensao == ".json" || contentType.Contains("json"))
        {
            return "JSON";
        }
        return null;
    }

    private static (bool Valido, string Motivo) ValidarArquivo(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return (false, "Nenhum arquivo foi fornecido.");
        }
        if (file.Length > MaxFileSize)
        {
            return (false, $"Arquivo muito grande. Tamanho maximo: {MaxFileSize / 1024 / 1024} MB.");
        }
        var extensao = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extensao != ".csv" && extensao != ".json")
        {
            return (false, "Apenas arquivos CSV e JSON sao aceitos.");
        }
        return (true, string.Empty);
    }
}
