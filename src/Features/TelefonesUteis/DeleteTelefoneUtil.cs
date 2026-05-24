using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public static class DeleteTelefoneUtil
{
    public static RouteHandlerBuilder MapDeleteTelefoneUtil(this RouteGroupBuilder group)
    {
        return group.MapDelete("/{id:guid}", Handle)
            .WithName(nameof(DeleteTelefoneUtil));
    }

    private static async Task<NoContent> Handle(
        Guid id,
        ITelefoneUtilService service,
        CancellationToken cancellationToken)
    {
        // 404 via NotFoundException -> ProblemDetailsMiddleware.
        await service.RemoverAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }
}
