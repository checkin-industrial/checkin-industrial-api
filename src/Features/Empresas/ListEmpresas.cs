using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public static class ListEmpresas
{
    public static RouteHandlerBuilder MapListEmpresas(this RouteGroupBuilder group)
    {
        return group.MapGet("/", Handle)
            .WithName(nameof(ListEmpresas));
    }

    private static async Task<Ok<List<EmpresaMapDTO>>> Handle(
        IEmpresaMapService service,
        CancellationToken cancellationToken)
    {
        var empresas = await service.ListarParaMapaAsync(cancellationToken);
        return TypedResults.Ok(empresas.ToList());
    }
}
