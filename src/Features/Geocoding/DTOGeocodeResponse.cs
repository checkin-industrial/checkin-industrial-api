namespace AppTurismoIndustrial.Api.Features.Geocoding;

public class DTOGeocodeResponse
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string Accuracy { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
}
