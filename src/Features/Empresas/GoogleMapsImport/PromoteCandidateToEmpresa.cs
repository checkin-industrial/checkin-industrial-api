using AppTurismoIndustrial.Api.Shared.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

public static class PromoteCandidateToEmpresa
{
    public static RouteGroupBuilder MapPromoteCandidateToEmpresa(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/promote-empresa", Handle)
            .WithName(nameof(PromoteCandidateToEmpresa))
            .AddEndpointFilter<ValidationFilter<DTOEmpresaCriar>>();
        return group;
    }

    private static async Task<Created<DTORespostaEmpresa>> Handle(
        Guid id,
        DTOEmpresaCriar dto,
        IImportCandidateService service,
        CancellationToken cancellationToken)
    {
        var empresa = await service.PromoteToEmpresaAsync(id, dto, cancellationToken);
        return TypedResults.Created($"/api/empresas/{empresa.Id}", empresa);
    }
}
