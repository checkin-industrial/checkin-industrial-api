
namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public interface IPontoInstitucionalQuery
{
    Task<IReadOnlyCollection<DTOPontoInstitucional>> ConsultarAsync(
        DTOPontoInstitucionalFiltroParams filtros,
        CancellationToken cancellationToken = default);
}
