
namespace AppTurismoIndustrial.Api.Features.Empresas;

public interface IEmpresaFilterService
{
    Task<IReadOnlyCollection<EmpresaFilterDTO>> FiltrarAsync(
        EmpresaFilterParams filtros,
        CancellationToken cancellationToken = default);
}