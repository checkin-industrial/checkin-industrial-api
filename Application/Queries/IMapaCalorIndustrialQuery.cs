using AppTurismoIndustrial.Api.Application.DTOs.Analytics;
using AppTurismoIndustrial.Api.Domain.Entities;

namespace AppTurismoIndustrial.Api.Application.Queries;

public interface IMapaCalorIndustrialQuery
{
    Task<IReadOnlyCollection<HeatmapQueryPoint>> ObterPontosAsync(
        string? cnae,
        SetorEmpresa? setor,
        CancellationToken cancellationToken = default);
}