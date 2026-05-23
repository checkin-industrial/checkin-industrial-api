using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Geocoding;

public static class GeocodeAddress
{
    public static IEndpointRouteBuilder MapGeocodeAddress(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/empresas/geocode", Handle)
            .WithName(nameof(GeocodeAddress))
            .WithTags("Geocoding");
        return endpoints;
    }

    private static async Task<Results<Ok<DTOGeocodeResponse>, BadRequest<object>, NotFound<object>>> Handle(
        DTOGeocodeRequest dto,
        IGeocodingService service,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.Endereco))
        {
            return TypedResults.BadRequest<object>(new { message = "Endereco e obrigatorio para geocodificacao." });
        }

        var result = await service.GeocodeAsync(
            dto.Endereco,
            dto.Municipio,
            dto.Estado,
            cancellationToken);

        if (result is null)
        {
            return TypedResults.NotFound<object>(new { message = "Nao foi possivel geocodificar o endereco informado." });
        }

        return TypedResults.Ok(new DTOGeocodeResponse
        {
            Latitude = result.Latitude,
            Longitude = result.Longitude,
            Accuracy = result.Accuracy,
            Provider = result.Provider,
        });
    }
}
