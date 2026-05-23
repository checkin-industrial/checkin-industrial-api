using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public static class GetPontoInstitucionalById
{
    public static RouteGroupBuilder MapGetPontoInstitucionalById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", Handle)
            .WithName(nameof(GetPontoInstitucionalById));
        return group;
    }

    private static async Task<Results<Ok<DTOPontoInstitucional>, NotFound>> Handle(
        Guid id,
        IPontoInstitucionalService service,
        CancellationToken cancellationToken)
    {
        var ponto = await service.ObterPorIdAsync(id, cancellationToken);
        return ponto is null ? TypedResults.NotFound() : TypedResults.Ok(ponto);
    }
}
