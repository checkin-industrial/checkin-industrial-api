using AppTurismoIndustrial.Api.Shared.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

public static class PromoteCandidateToTelefone
{
    public static RouteGroupBuilder MapPromoteCandidateToTelefone(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/promote-telefone", Handle)
            .WithName(nameof(PromoteCandidateToTelefone))
            .AddEndpointFilter<ValidationFilter<DTOTelefoneUtilCriar>>();
        return group;
    }

    private static async Task<Created<DTOTelefoneUtil>> Handle(
        Guid id,
        DTOTelefoneUtilCriar dto,
        IImportCandidateService service,
        CancellationToken cancellationToken)
    {
        var telefone = await service.PromoteToTelefoneAsync(id, dto, cancellationToken);
        return TypedResults.Created($"/api/telefones-uteis/{telefone.Id}", telefone);
    }
}
