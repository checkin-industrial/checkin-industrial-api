using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public static class UpdateTelefoneUtil
{
    public static RouteGroupBuilder MapUpdateTelefoneUtil(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", Handle)
            .WithName(nameof(UpdateTelefoneUtil));
        return group;
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
