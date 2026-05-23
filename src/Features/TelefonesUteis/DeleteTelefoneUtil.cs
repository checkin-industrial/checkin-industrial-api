using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public static class DeleteTelefoneUtil
{
    public static RouteHandlerBuilder MapDeleteTelefoneUtil(this RouteGroupBuilder group)
    {
        return group.MapDelete("/{id:guid}", Handle)
            .WithName(nameof(DeleteTelefoneUtil));
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
