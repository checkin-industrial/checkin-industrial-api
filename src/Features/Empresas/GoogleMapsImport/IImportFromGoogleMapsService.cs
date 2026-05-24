namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

public interface IImportFromGoogleMapsService
{
    Task<DTOImportFromGoogleMapsResponse> ImportAsync(
        DTOImportFromGoogleMapsRequest request,
        CancellationToken cancellationToken = default);
}
