using System.Text.Json.Serialization;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public class DTOUploadArquivoResponse
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}
