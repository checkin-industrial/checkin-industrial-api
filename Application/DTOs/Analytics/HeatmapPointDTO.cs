using System.Text.Json.Serialization;

namespace AppTurismoIndustrial.Api.Application.DTOs.Analytics;

public class HeatmapPointDTO
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("peso")]
    public int Peso { get; set; }
}