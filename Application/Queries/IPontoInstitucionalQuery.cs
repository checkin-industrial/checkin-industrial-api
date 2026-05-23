using AppTurismoIndustrial.Api.Application.DTOs;

namespace AppTurismoIndustrial.Api.Application.Queries;

public interface IPontoInstitucionalQuery
{
    Task<IReadOnlyCollection<DTOPontoInstitucional>> ConsultarAsync(
        DTOPontoInstitucionalFiltroParams filtros,
        CancellationToken cancellationToken = default);
}
