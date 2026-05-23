using AppTurismoIndustrial.Api.Application.DTOs;

namespace AppTurismoIndustrial.Api.Application.Services;

public interface ITelefoneUtilService
{
    Task<IReadOnlyCollection<DTOTelefoneUtil>> ListarAsync(
        DTOTelefoneUtilFiltroParams filtros,
        CancellationToken cancellationToken = default);

    Task<DTOTelefoneUtil?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DTOTelefoneUtil> CriarAsync(DTOTelefoneUtilCriar dto, CancellationToken cancellationToken = default);

    Task<bool> AtualizarAsync(Guid id, DTOTelefoneUtilAtualizar dto, CancellationToken cancellationToken = default);

    Task<bool> RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
