using AppTurismoIndustrial.Api.Application.DTOs;

namespace AppTurismoIndustrial.Api.Application.Services;

public interface IEmpresaNeighborhoodService
{
    Task<DTOEmpresaVizinhancaResponse?> ObterVizinhancaAsync(
        Guid empresaId,
        int radiusMeters,
        int limit,
        CancellationToken cancellationToken = default);
}