using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

public static class GetImportCandidateById
{
    public static RouteGroupBuilder MapGetImportCandidateById(this RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", Handle).WithName(nameof(GetImportCandidateById));
        return group;
    }

    private static async Task<Results<Ok<DTOImportCandidateResponse>, NotFound>> Handle(
        Guid id,
        IImportCandidateService service,
        CancellationToken cancellationToken)
    {
        var candidate = await service.GetByIdAsync(id, cancellationToken);
        return candidate is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(candidate);
    }
}
