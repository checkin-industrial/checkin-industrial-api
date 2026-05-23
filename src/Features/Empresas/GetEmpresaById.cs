using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public static class GetEmpresaById
{
    public static RouteGroupBuilder MapGetEmpresaById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", Handle)
            .WithName(nameof(GetEmpresaById));
        return group;
    }

    private static async Task<Results<Ok<DTORespostaEmpresa>, NotFound>> Handle(
        Guid id,
        IEmpresaService service)
    {
        var empresa = await service.ObterPorIdAsync(id);
        return empresa is null ? TypedResults.NotFound() : TypedResults.Ok(empresa);
    }
}
