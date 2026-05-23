
namespace AppTurismoIndustrial.Api.Features.Empresas;

public interface IEmpresaMapService
{
    Task<IReadOnlyCollection<EmpresaMapDTO>> ListarParaMapaAsync(CancellationToken cancellationToken = default);
}