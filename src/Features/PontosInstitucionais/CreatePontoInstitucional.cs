using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public static class CreatePontoInstitucional
{
    public static RouteHandlerBuilder MapCreatePontoInstitucional(this RouteGroupBuilder group)
    {
        return group.MapPost("/", Handle)
            .WithName(nameof(CreatePontoInstitucional));
    }

    private static async Task<Created<DTOPontoInstitucional>> Handle(
        DTOPontoInstitucionalCriar dto,
        IPontoInstitucionalService service,
        CancellationToken cancellationToken)
    {
        var pontoCriado = await service.CriarAsync(dto, cancellationToken);
        return TypedResults.Created($"/api/pontos-institucionais/{pontoCriado.Id}", pontoCriado);
    }
}
