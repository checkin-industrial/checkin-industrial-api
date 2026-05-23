using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public static class CreateTelefoneUtil
{
    public static RouteGroupBuilder MapCreateTelefoneUtil(this RouteGroupBuilder group)
    {
        group.MapPost("/", Handle)
            .WithName(nameof(CreateTelefoneUtil));
        return group;
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
