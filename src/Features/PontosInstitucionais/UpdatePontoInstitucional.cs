using AppTurismoIndustrial.Api.Shared.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public static class UpdatePontoInstitucional
{
    public static RouteHandlerBuilder MapUpdatePontoInstitucional(this RouteGroupBuilder group)
    {
        return group.MapPut("/{id:guid}", Handle)
            .WithName(nameof(UpdatePontoInstitucional))
            .AddEndpointFilter<ValidationFilter<DTOPontoInstitucionalAtualizar>>();
    }

    private static async Task<NoContent> Handle(
        Guid id,
        DTOPontoInstitucionalAtualizar dto,
        IPontoInstitucionalService service,
        CancellationToken cancellationToken)
    {
        // 404 via NotFoundException -> ProblemDetailsMiddleware.
        await service.AtualizarAsync(id, dto, cancellationToken);
        return TypedResults.NoContent();
    }
}
