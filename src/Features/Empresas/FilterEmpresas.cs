using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public static class FilterEmpresas
{
    public static RouteHandlerBuilder MapFilterEmpresas(this RouteGroupBuilder group)
    {
        return group.MapGet("/filter", Handle)
            .WithName(nameof(FilterEmpresas));
    }

    private static async Task<Results<Ok<List<EmpresaFilterDTO>>, BadRequest<object>>> Handle(
        [AsParameters] EmpresaFilterParams filtros,
        IEmpresaFilterService service,
        CancellationToken cancellationToken)
    {
        if (filtros.MinFuncionarios.HasValue
            && filtros.MaxFuncionarios.HasValue
            && filtros.MinFuncionarios > filtros.MaxFuncionarios)
        {
            return TypedResults.BadRequest<object>(new
            {
                message = "minFuncionarios nao pode ser maior que maxFuncionarios."
            });
        }

        var empresasFiltradas = await service.FiltrarAsync(filtros, cancellationToken);
        return TypedResults.Ok(empresasFiltradas.ToList());
    }
}
