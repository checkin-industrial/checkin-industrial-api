using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public static class ListTelefonesUteis
{
    public static RouteGroupBuilder MapListTelefonesUteis(this RouteGroupBuilder group)
    {
        group.MapGet("/", Handle)
            .WithName(nameof(ListTelefonesUteis));
        return group;
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
