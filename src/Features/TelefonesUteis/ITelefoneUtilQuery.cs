
namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public interface ITelefoneUtilQuery
{
    Task<IReadOnlyCollection<DTOTelefoneUtil>> ConsultarAsync(
        DTOTelefoneUtilFiltroParams filtros,
        CancellationToken cancellationToken = default);
}
