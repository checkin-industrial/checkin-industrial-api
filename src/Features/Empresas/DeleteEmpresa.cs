using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public static class DeleteEmpresa
{
    public static RouteGroupBuilder MapDeleteEmpresa(this RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", Handle)
            .WithName(nameof(DeleteEmpresa));
        return group;
    }

    private static async Task<Results<NoContent, NotFound>> Handle(
        Guid id,
        IEmpresaService service)
    {
        var removida = await service.RemoverAsync(id);
        return removida ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}
