using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public static class ListTelefonesUteis
{
    public static RouteHandlerBuilder MapListTelefonesUteis(this RouteGroupBuilder group)
    {
        return group.MapGet("/", Handle)
            .WithName(nameof(ListTelefonesUteis));
    }

    private static async Task<Ok<List<DTOTelefoneUtil>>> Handle(
        [AsParameters] DTOTelefoneUtilFiltroParams filtros,
        ITelefoneUtilService service,
        CancellationToken cancellationToken)
    {
        var telefones = await service.ListarAsync(filtros, cancellationToken);
        return TypedResults.Ok(telefones.ToList());
    }
}
