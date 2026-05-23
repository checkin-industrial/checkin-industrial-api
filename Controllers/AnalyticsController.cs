using AppTurismoIndustrial.Api.Application.DTOs.Analytics;
using AppTurismoIndustrial.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace AppTurismoIndustrial.Api.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IHeatmapService _heatmapService;
    private readonly ILogger<AnalyticsController> _logger;

    private static readonly HashSet<string> SetoresValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "industria",
        "comercio",
        "servicos"
    };

    public AnalyticsController(
        IHeatmapService heatmapService,
        ILogger<AnalyticsController> logger)
    {
        _heatmapService = heatmapService;
        _logger = logger;
    }

    [HttpGet("heatmap")]
    [ProducesResponseType(typeof(List<HeatmapPointDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<HeatmapPointDTO>>> ObterMapaCalor(
        [FromQuery] DTOConsultaMapaCalorIndustrial consulta,
        CancellationToken cancellationToken = default)
    {
        ValidarConsulta(consulta);
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var pontos = await _heatmapService.ObterHeatmapAsync(consulta, cancellationToken);
            return Ok(pontos.ToList());
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Consulta de heatmap cancelada pelo cliente.");
            return Problem(
                detail: "Consulta cancelada.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Requisição cancelada");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar heatmap industrial.");
            return Problem(
                detail: "Erro interno ao processar o mapa de calor.",
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Erro interno");
        }
    }

    private void ValidarConsulta(DTOConsultaMapaCalorIndustrial consulta)
    {
        if (!string.IsNullOrWhiteSpace(consulta.Cnae))
        {
            var somenteNumeros = Regex.Replace(consulta.Cnae, @"\D", string.Empty);
            if (somenteNumeros.Length is < 4 or > 7)
            {
                ModelState.AddModelError(
                    nameof(consulta.Cnae),
                    "O CNAE deve conter entre 4 e 7 dígitos numéricos.");
            }
        }

        if (!string.IsNullOrWhiteSpace(consulta.Setor)
            && !SetoresValidos.Contains(consulta.Setor.Trim()))
        {
            ModelState.AddModelError(
                nameof(consulta.Setor),
                "Setor inválido. Valores aceitos: industria, comercio, servicos.");
        }
    }
}