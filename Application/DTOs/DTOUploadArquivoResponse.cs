using System.Text.Json.Serialization;

namespace AppTurismoIndustrial.Api.Application.DTOs;

public class DTOUploadArquivoResponse
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
