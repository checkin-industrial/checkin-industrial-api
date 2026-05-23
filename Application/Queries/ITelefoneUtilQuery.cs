using AppTurismoIndustrial.Api.Application.DTOs;

namespace AppTurismoIndustrial.Api.Application.Queries;

public interface ITelefoneUtilQuery
{
    Task<IReadOnlyCollection<DTOTelefoneUtil>> ConsultarAsync(
        DTOTelefoneUtilFiltroParams filtros,
        CancellationToken cancellationToken = default);
}
