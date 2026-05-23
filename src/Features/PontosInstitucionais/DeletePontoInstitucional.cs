using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public static class DeletePontoInstitucional
{
    public static RouteHandlerBuilder MapDeletePontoInstitucional(this RouteGroupBuilder group)
    {
        return group.MapDelete("/{id:guid}", Handle)
            .WithName(nameof(DeletePontoInstitucional));
    }

    private static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        IPontoInstitucionalService service,
        CancellationToken cancellationToken)
    {
        var removido = await service.RemoverAsync(id, cancellationToken);
        return removido ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
