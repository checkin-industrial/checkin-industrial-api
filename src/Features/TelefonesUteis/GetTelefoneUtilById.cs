using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public static class GetTelefoneUtilById
{
    public static RouteGroupBuilder MapGetTelefoneUtilById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", Handle)
            .WithName(nameof(GetTelefoneUtilById));
        return group;
    }

    private static async Task<Results<Ok<DTOTelefoneUtil>, NotFound>> Handle(
        Guid id,
        ITelefoneUtilService service,
        CancellationToken cancellationToken)
    {
        var telefoneUtil = await service.ObterPorIdAsync(id, cancellationToken);

        if (telefoneUtil is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(telefoneUtil);
    }
}
