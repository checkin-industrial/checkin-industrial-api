using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AppTurismoIndustrial.Api.Features.Analytics;

public static class GetHeatmap
{
    private static readonly HashSet<string> SetoresValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "industria", "comercio", "servicos"
    };

    public static RouteHandlerBuilder MapGetHeatmap(this RouteGroupBuilder group)
    {
        return group.MapGet("/heatmap", Handle)
            .WithName(nameof(GetHeatmap));
    }

    private static async Task<Results<Ok<List<HeatmapPointDTO>>, ValidationProblem, ProblemHttpResult>> Handle(
        [AsParameters] DTOConsultaMapaCalorIndustrial consulta,
        IHeatmapService heatmapService,
        ILogger<DTOConsultaMapaCalorIndustrial> logger,
        CancellationToken cancellationToken)
    {
        var errors = ValidarConsulta(consulta);
        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors);
        }

        try
        {
            var pontos = await heatmapService.ObterHeatmapAsync(consulta, cancellationToken);
            return TypedResults.Ok(pontos.ToList());
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Consulta de heatmap cancelada pelo cliente.");
            return TypedResults.Problem(
                detail: "Consulta cancelada.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Requisicao cancelada");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao consultar heatmap industrial.");
            return TypedResults.Problem(
                detail: "Erro interno ao processar o mapa de calor.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Erro interno");
        }
    }

    private static Dictionary<string, string[]> ValidarConsulta(DTOConsultaMapaCalorIndustrial consulta)
    {
        var errors = new Dictionary<string, string[]>();

        if (!string.IsNullOrWhiteSpace(consulta.Cnae))
        {
            var somenteNumeros = Regex.Replace(consulta.Cnae, @"\D", string.Empty);
            if (somenteNumeros.Length is < 4 or > 7)
            {
                errors[nameof(consulta.Cnae)] = new[] { "O CNAE deve conter entre 4 e 7 digitos numericos." };
            }
        }

        if (!string.IsNullOrWhiteSpace(consulta.Setor)
            && !SetoresValidos.Contains(consulta.Setor.Trim()))
        {
            errors[nameof(consulta.Setor)] = new[] { "Setor invalido. Valores aceitos: industria, comercio, servicos." };
        }

        return errors;
    }
}
