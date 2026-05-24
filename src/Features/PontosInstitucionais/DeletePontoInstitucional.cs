using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public static class DeletePontoInstitucional
{
    public static RouteHandlerBuilder MapDeletePontoInstitucional(this RouteGroupBuilder group)
    {
        return group.MapDelete("/{id:guid}", Handle)
            .WithName(nameof(DeletePontoInstitucional));
    }

    private static async Task<NoContent> Handle(
        Guid id,
        IPontoInstitucionalService service,
        CancellationToken cancellationToken)
    {
        // 404 via NotFoundException -> ProblemDetailsMiddleware.
        await service.RemoverAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }
}
