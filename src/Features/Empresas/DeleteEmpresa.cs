using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public static class DeleteEmpresa
{
    public static RouteHandlerBuilder MapDeleteEmpresa(this RouteGroupBuilder group)
    {
        return group.MapDelete("/{id:guid}", Handle)
            .WithName(nameof(DeleteEmpresa));
    }

    private static async Task<NoContent> Handle(
        Guid id,
        IEmpresaService service,
        CancellationToken cancellationToken)
    {
        // 404 via NotFoundException -> ProblemDetailsMiddleware.
        await service.RemoverAsync(id, cancellationToken);
        return TypedResults.NoContent();
    }
}
