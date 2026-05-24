
namespace AppTurismoIndustrial.Api.Features.Empresas;

public interface IEmpresaService
{
    Task<DTORespostaEmpresa> CriarAsync(DTOEmpresaCriar dto, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<DTORespostaEmpresa>> ListarAsync(CancellationToken cancellationToken = default);
    Task<DTORespostaEmpresa?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DTORespostaEmpresa> AtualizarAsync(Guid id, DTOEmpresaAtualizar dto, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
