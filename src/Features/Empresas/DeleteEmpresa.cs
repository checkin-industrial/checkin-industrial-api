using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public static class DeleteEmpresa
{
    public static RouteHandlerBuilder MapDeleteEmpresa(this RouteGroupBuilder group)
    {
        return group.MapDelete("/{id:guid}", Handle)
            .WithName(nameof(DeleteEmpresa));
    }

    private static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        IEmpresaService service)
    {
        var removida = await service.RemoverAsync(id);
        return removida ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
