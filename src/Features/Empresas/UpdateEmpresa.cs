using AppTurismoIndustrial.Api.Shared.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public static class UpdateEmpresa
{
    public static RouteHandlerBuilder MapUpdateEmpresa(this RouteGroupBuilder group)
    {
        return group.MapPut("/{id:guid}", Handle)
            .WithName(nameof(UpdateEmpresa))
            .AddEndpointFilter<ValidationFilter<DTOEmpresaAtualizar>>();
    }

    private static async Task<NoContent> Handle(
        Guid id,
        DTOEmpresaAtualizar dto,
        IEmpresaService service,
        CancellationToken cancellationToken)
    {
        // 404 (empresa nao encontrada) e 409 (CNPJ duplicado) via excecoes ->
        // ProblemDetailsMiddleware. Manter contrato HTTP original.
        await service.AtualizarAsync(id, dto, cancellationToken);
        return TypedResults.NoContent();
    }
}
