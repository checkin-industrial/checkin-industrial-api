using System.Text.Json;
using AppTurismoIndustrial.Api.Shared.Errors;
using Microsoft.AspNetCore.Mvc;

namespace AppTurismoIndustrial.Api.Shared.Middleware;

/// <summary>
/// Captura excecoes nao tratadas e converte para ProblemDetails (RFC 7807).
/// AppException subclasses sao mapeadas para seus StatusCodes; demais viram 500.
/// </summary>
public sealed class ProblemDetailsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ProblemDetailsMiddleware(
        RequestDelegate next,
        ILogger<ProblemDetailsMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "AppException: {Message}", ex.Message);
            await WriteProblemAsync(context, ex.StatusCode, ex.Message, title: GetTitle(ex.StatusCode));
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation("Request cancelada pelo cliente: {Path}", context.Request.Path);
            await WriteProblemAsync(context, StatusCodes.Status499ClientClosedRequest, "Request cancelada pelo cliente.", title: "Cancelada");
            return;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro nao tratado: {Path}", context.Request.Path);
            var detail = _environment.IsDevelopment()
                ? $"{ex.GetType().Name}: {ex.Message}"
                : "Erro interno do servidor.";
            await WriteProblemAsync(context, StatusCodes.Status500InternalServerError, detail, title: "Erro interno");
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string detail, string title)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
        };

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(context.Response.Body, problem);
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Requisicao invalida",
        StatusCodes.Status404NotFound => "Recurso nao encontrado",
        StatusCodes.Status409Conflict => "Conflito",
        _ => "Erro"
    };
}

public static class ProblemDetailsMiddlewareExtensions
{
    public static IApplicationBuilder UseProblemDetailsMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ProblemDetailsMiddleware>();
    }
}
