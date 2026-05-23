using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public static class ListPontosInstitucionais
{
    public static RouteHandlerBuilder MapListPontosInstitucionais(this RouteGroupBuilder group)
    {
        return group.MapGet("/", Handle)
            .WithName(nameof(ListPontosInstitucionais));
    }

    private static async Task<Ok<List<DTOPontoInstitucional>>> Handle(
        [AsParameters] DTOPontoInstitucionalFiltroParams filtros,
        IPontoInstitucionalService service,
        CancellationToken cancellationToken)
    {
        var pontos = await service.ListarAsync(filtros, cancellationToken);
        return TypedResults.Ok(pontos.ToList());
    }
}
