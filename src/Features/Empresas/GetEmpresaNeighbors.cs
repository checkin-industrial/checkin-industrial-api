using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public static class GetEmpresaNeighbors
{
    public static RouteGroupBuilder MapGetEmpresaNeighbors(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}/neighbors", Handle)
            .WithName(nameof(GetEmpresaNeighbors));
        return group;
    }

    private static async Task<Results<Ok<DTOEmpresaVizinhancaResponse>, BadRequest<object>, NotFound>> Handle(
        Guid id,
        IEmpresaNeighborhoodService service,
        CancellationToken cancellationToken,
        int radius = 5000,
        int limit = 20)
    {
        if (radius <= 0)
        {
            return TypedResults.BadRequest<object>(new { message = "radius deve ser maior que zero." });
        }

        if (limit <= 0)
        {
            return TypedResults.BadRequest<object>(new { message = "limit deve ser maior que zero." });
        }

        var vizinhanca = await service.ObterVizinhancaAsync(id, radius, limit, cancellationToken);

        return vizinhanca is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(vizinhanca);
    }
}
