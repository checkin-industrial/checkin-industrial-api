
namespace AppTurismoIndustrial.Api.Features.Empresas;

public interface IEmpresaService
{
    Task<(DTORespostaEmpresa? empresa, bool cnpjDuplicado)> CriarAsync(DTOEmpresaCriar dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DTORespostaEmpresa>> ListarAsync(CancellationToken cancellationToken = default);
    Task<DTORespostaEmpresa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(bool atualizado, bool naoEncontrada, bool cnpjDuplicado)> AtualizarAsync(Guid id, DTOEmpresaAtualizar dto, CancellationToken cancellationToken = default);
    Task<bool> RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
