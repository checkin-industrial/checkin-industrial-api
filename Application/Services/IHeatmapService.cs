using AppTurismoIndustrial.Api.Application.DTOs.Analytics;

namespace AppTurismoIndustrial.Api.Application.Services;

public interface IHeatmapService
{
    Task<IReadOnlyCollection<HeatmapPointDTO>> ObterHeatmapAsync(
        DTOConsultaMapaCalorIndustrial consulta,
        CancellationToken cancellationToken = default);
}