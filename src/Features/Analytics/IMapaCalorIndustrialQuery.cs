

namespace AppTurismoIndustrial.Api.Features.Analytics;

public interface IMapaCalorIndustrialQuery
{
    Task<IReadOnlyCollection<HeatmapQueryPoint>> ObterPontosAsync(
        string? cnae,
        SetorEmpresa? setor,
        CancellationToken cancellationToken = default);
}