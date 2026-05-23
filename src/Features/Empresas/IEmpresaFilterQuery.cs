
namespace AppTurismoIndustrial.Api.Features.Empresas;

public interface IEmpresaFilterQuery
{
    Task<IReadOnlyCollection<EmpresaFilterDTO>> ConsultarAsync(
        EmpresaFilterParams filtros,
    int limit,
        CancellationToken cancellationToken = default);
}