using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public static class ListEmpresas
{
    public static RouteGroupBuilder MapListEmpresas(this RouteGroupBuilder group)
    {
        group.MapGet("/", Handle)
            .WithName(nameof(ListEmpresas));
        return group;
    }

    private static async Task<Ok<List<EmpresaMapDTO>>> Handle(
        IEmpresaMapService service,
        CancellationToken cancellationToken)
    {
        var empresas = await service.ListarParaMapaAsync(cancellationToken);
        return TypedResults.Ok(empresas.ToList());
    }
}
