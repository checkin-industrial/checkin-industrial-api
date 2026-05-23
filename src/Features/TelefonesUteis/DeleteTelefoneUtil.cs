using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public static class DeleteTelefoneUtil
{
    public static RouteGroupBuilder MapDeleteTelefoneUtil(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", Handle)
            .WithName(nameof(DeleteTelefoneUtil));
        return group;
    }

    private static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        ITelefoneUtilService service,
        CancellationToken cancellationToken)
    {
        var removido = await service.RemoverAsync(id, cancellationToken);
        return removido ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
