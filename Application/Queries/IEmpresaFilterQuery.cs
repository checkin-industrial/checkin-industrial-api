using AppTurismoIndustrial.Api.Application.DTOs;

namespace AppTurismoIndustrial.Api.Application.Queries;

public interface IEmpresaFilterQuery
{
    Task<IReadOnlyCollection<EmpresaFilterDTO>> ConsultarAsync(
        EmpresaFilterParams filtros,
    int limit,
        CancellationToken cancellationToken = default);
}