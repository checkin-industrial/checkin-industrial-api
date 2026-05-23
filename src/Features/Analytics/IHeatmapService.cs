
namespace AppTurismoIndustrial.Api.Features.Analytics;

public interface IHeatmapService
{
    Task<IReadOnlyCollection<HeatmapPointDTO>> ObterHeatmapAsync(
        DTOConsultaMapaCalorIndustrial consulta,
        CancellationToken cancellationToken = default);
}