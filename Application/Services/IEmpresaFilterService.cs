using AppTurismoIndustrial.Api.Application.DTOs;

namespace AppTurismoIndustrial.Api.Application.Services;

public interface IEmpresaFilterService
{
    Task<IReadOnlyCollection<EmpresaFilterDTO>> FiltrarAsync(
        EmpresaFilterParams filtros,
        CancellationToken cancellationToken = default);
}