using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public static class DeletePontoInstitucional
{
    public static RouteGroupBuilder MapDeletePontoInstitucional(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", Handle)
            .WithName(nameof(DeletePontoInstitucional));
        return group;
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
