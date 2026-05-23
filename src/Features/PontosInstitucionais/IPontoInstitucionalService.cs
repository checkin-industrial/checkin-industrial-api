
namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public interface IPontoInstitucionalService
{
    Task<IReadOnlyCollection<DTOPontoInstitucional>> ListarAsync(
        DTOPontoInstitucionalFiltroParams filtros,
        CancellationToken cancellationToken = default);

    Task<DTOPontoInstitucional?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DTOPontoInstitucional> CriarAsync(DTOPontoInstitucionalCriar dto, CancellationToken cancellationToken = default);

    Task<bool> AtualizarAsync(Guid id, DTOPontoInstitucionalAtualizar dto, CancellationToken cancellationToken = default);

    Task<bool> RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
