using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public static class UpdateTelefoneUtil
{
    public static RouteHandlerBuilder MapUpdateTelefoneUtil(this RouteGroupBuilder group)
    {
        return group.MapPut("/{id:guid}", Handle)
            .WithName(nameof(UpdateTelefoneUtil));
    }

    private static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        DTOTelefoneUtilAtualizar dto,
        ITelefoneUtilService service,
        CancellationToken cancellationToken)
    {
        var atualizado = await service.AtualizarAsync(id, dto, cancellationToken);
        return atualizado ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
