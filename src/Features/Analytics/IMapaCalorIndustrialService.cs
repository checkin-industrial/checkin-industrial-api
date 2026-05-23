
namespace AppTurismoIndustrial.Api.Features.Analytics;

public interface IMapaCalorIndustrialService
{
    Task<IReadOnlyCollection<HeatmapPointDTO>> ObterMapaCalorAsync(
        DTOConsultaMapaCalorIndustrial consulta,
        CancellationToken cancellationToken = default);
}