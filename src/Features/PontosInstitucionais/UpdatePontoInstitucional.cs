using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public static class UpdatePontoInstitucional
{
    public static RouteGroupBuilder MapUpdatePontoInstitucional(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", Handle)
            .WithName(nameof(UpdatePontoInstitucional));
        return group;
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
