using AppTurismoIndustrial.Api.Application.DTOs;

namespace AppTurismoIndustrial.Api.Application.Services;

public interface IEmpresaMapService
{
    Task<IReadOnlyCollection<EmpresaMapDTO>> ListarParaMapaAsync(CancellationToken cancellationToken = default);
}