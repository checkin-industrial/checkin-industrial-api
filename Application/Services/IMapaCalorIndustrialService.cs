using AppTurismoIndustrial.Api.Application.DTOs.Analytics;

namespace AppTurismoIndustrial.Api.Application.Services;

public interface IMapaCalorIndustrialService
{
    Task<IReadOnlyCollection<HeatmapPointDTO>> ObterMapaCalorAsync(
        DTOConsultaMapaCalorIndustrial consulta,
        CancellationToken cancellationToken = default);
}