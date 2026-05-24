using AppTurismoIndustrial.Api.Shared.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public static class UpdateTelefoneUtil
{
    public static RouteHandlerBuilder MapUpdateTelefoneUtil(this RouteGroupBuilder group)
    {
        return group.MapPut("/{id:guid}", Handle)
            .WithName(nameof(UpdateTelefoneUtil))
            .AddEndpointFilter<ValidationFilter<DTOTelefoneUtilAtualizar>>();
    }

    private static async Task<NoContent> Handle(
        Guid id,
        DTOTelefoneUtilAtualizar dto,
        ITelefoneUtilService service,
        CancellationToken cancellationToken)
    {
        // 404 via NotFoundException -> ProblemDetailsMiddleware.
        await service.AtualizarAsync(id, dto, cancellationToken);
        return TypedResults.NoContent();
    }
}
