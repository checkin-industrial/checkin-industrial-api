using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public static class CreatePontoInstitucional
{
    public static RouteGroupBuilder MapCreatePontoInstitucional(this RouteGroupBuilder group)
    {
        group.MapPost("/", Handle)
            .WithName(nameof(CreatePontoInstitucional));
        return group;
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
