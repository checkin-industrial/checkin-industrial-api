using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public static class UpdatePontoInstitucional
{
    public static RouteHandlerBuilder MapUpdatePontoInstitucional(this RouteGroupBuilder group)
    {
        return group.MapPut("/{id:guid}", Handle)
            .WithName(nameof(UpdatePontoInstitucional));
    }

    private static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        DTOPontoInstitucionalAtualizar dto,
        IPontoInstitucionalService service,
        CancellationToken cancellationToken)
    {
        var atualizado = await service.AtualizarAsync(id, dto, cancellationToken);
        return atualizado ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
