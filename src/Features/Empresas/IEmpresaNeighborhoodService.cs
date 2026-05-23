
namespace AppTurismoIndustrial.Api.Features.Empresas;

public interface IEmpresaNeighborhoodService
{
    Task<DTOEmpresaVizinhancaResponse?> ObterVizinhancaAsync(
        Guid empresaId,
        int radiusMeters,
        int limit,
        CancellationToken cancellationToken = default);
}