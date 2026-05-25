using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace AppTurismoIndustrial.Api.Features.Analytics;

public class HeatmapService : IHeatmapService
{
    private readonly IMapaCalorIndustrialQuery _mapaCalorIndustrialQuery;
    private readonly int _maxPointsForFrontend;

    public HeatmapService(
        IMapaCalorIndustrialQuery mapaCalorIndustrialQuery,
        IOptions<LimitsOptions>? limits = null)
    {
        _mapaCalorIndustrialQuery = mapaCalorIndustrialQuery;
        _maxPointsForFrontend = (limits?.Value ?? new LimitsOptions()).MaxPointsForFrontend;
    }

    public async Task<IReadOnlyCollection<HeatmapPointDTO>> ObterHeatmapAsync(
        DTOConsultaMapaCalorIndustrial consulta,
        CancellationToken cancellationToken = default)
    {
        var cnae = NormalizarCnae(consulta.Cnae);
        var setor = ParseSetor(consulta.Setor);

        var pontos = await _mapaCalorIndustrialQuery.ObterPontosAsync(cnae, setor, cancellationToken);

        // Otimiza payload para renderização: limita volume e mantém precisão suficiente para mapa web.
        return pontos
            .Where(p => p.Density > 0)
            .OrderByDescending(p => p.Density)
            .Take(_maxPointsForFrontend)
            .Select(p => new HeatmapPointDTO
            {
                Latitude = Math.Round(p.Latitude, 6),
                Longitude = Math.Round(p.Longitude, 6),
                Peso = p.Density
            })
            .ToList();
    }

    private static string? NormalizarCnae(string? cnae)
    {
        if (string.IsNullOrWhiteSpace(cnae))
        {
            return null;
        }

        var somenteNumeros = Regex.Replace(cnae, @"\D", string.Empty);
        return string.IsNullOrWhiteSpace(somenteNumeros) ? null : somenteNumeros;
    }

    private static SetorEmpresa? ParseSetor(string? setor)
    {
        if (string.IsNullOrWhiteSpace(setor))
        {
            return null;
        }

        return setor.Trim().ToLowerInvariant() switch
        {
            "industria" => SetorEmpresa.Industria,
            "comercio" => SetorEmpresa.Comercio,
            "servicos" => SetorEmpresa.Servicos,
            _ => null
        };
    }
}