using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

public static class ImportFromGoogleMaps
{
    public static RouteGroupBuilder MapImportFromGoogleMaps(this RouteGroupBuilder group)
    {
        group.MapPost("/google-maps", Handle).WithName(nameof(ImportFromGoogleMaps));
        return group;
    }

    private static async Task<Ok<DTOImportFromGoogleMapsResponse>> Handle(
        DTOImportFromGoogleMapsRequest request,
        IImportFromGoogleMapsService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ImportAsync(request, cancellationToken);
        return TypedResults.Ok(result);
    }
}
