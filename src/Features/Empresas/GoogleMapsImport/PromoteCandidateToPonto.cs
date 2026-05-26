using AppTurismoIndustrial.Api.Shared.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

public static class PromoteCandidateToPonto
{
    public static RouteGroupBuilder MapPromoteCandidateToPonto(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/promote-ponto", Handle)
            .WithName(nameof(PromoteCandidateToPonto))
            .AddEndpointFilter<ValidationFilter<DTOPontoInstitucionalCriar>>();
        return group;
    }

    private static async Task<Created<DTOPontoInstitucional>> Handle(
        Guid id,
        DTOPontoInstitucionalCriar dto,
        IImportCandidateService service,
        CancellationToken cancellationToken)
    {
        var ponto = await service.PromoteToPontoAsync(id, dto, cancellationToken);
        return TypedResults.Created($"/api/pontos-institucionais/{ponto.Id}", ponto);
    }
}
