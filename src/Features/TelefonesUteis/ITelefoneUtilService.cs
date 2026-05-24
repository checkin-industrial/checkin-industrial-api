
namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public interface ITelefoneUtilService
{
    Task<IReadOnlyCollection<DTOTelefoneUtil>> ListarAsync(
        DTOTelefoneUtilFiltroParams filtros,
        CancellationToken cancellationToken = default);

    Task<DTOTelefoneUtil?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DTOTelefoneUtil> CriarAsync(DTOTelefoneUtilCriar dto, CancellationToken cancellationToken = default);

    Task AtualizarAsync(Guid id, DTOTelefoneUtilAtualizar dto, CancellationToken cancellationToken = default);

    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
