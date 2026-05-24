using AppTurismoIndustrial.Api.Shared.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public static class CreateTelefoneUtil
{
    public static RouteHandlerBuilder MapCreateTelefoneUtil(this RouteGroupBuilder group)
    {
        return group.MapPost("/", Handle)
            .WithName(nameof(CreateTelefoneUtil))
            .AddEndpointFilter<ValidationFilter<DTOTelefoneUtilCriar>>();
    }

    private static async Task<Created<DTOTelefoneUtil>> Handle(
        DTOTelefoneUtilCriar dto,
        ITelefoneUtilService service,
        CancellationToken cancellationToken)
    {
        var telefoneCriado = await service.CriarAsync(dto, cancellationToken);
        return TypedResults.Created($"/api/telefones-uteis/{telefoneCriado.Id}", telefoneCriado);
    }
}
