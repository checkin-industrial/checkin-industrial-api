using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public static class GetEmpresaById
{
    public static RouteHandlerBuilder MapGetEmpresaById(this RouteGroupBuilder group)
    {
        return group.MapGet("/{id:guid}", Handle)
            .WithName(nameof(GetEmpresaById));
    }

    private static async Task<Results<Ok<DTORespostaEmpresa>, NotFound>> Handle(
        Guid id,
        IEmpresaService service)
    {
        var empresa = await service.ObterPorIdAsync(id);
        return empresa is null ? TypedResults.NotFound() : TypedResults.Ok(empresa);
    }
}
