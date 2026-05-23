using System.Text.Json.Serialization;

namespace AppTurismoIndustrial.Api.Features.Empresas;

/// <summary>
/// Representa um marcador de empresa para renderização em mapa no frontend.
/// </summary>
public class EmpresaMapDTO
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("nomeFantasia")]
    public string NomeFantasia { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("setor")]
    public string Setor { get; set; } = string.Empty;

    [JsonPropertyName("cnae")]
    public string Cnae { get; set; } = string.Empty;

    [JsonPropertyName("descricaoCnae")]
    public string DescricaoCnae { get; set; } = string.Empty;

    [JsonPropertyName("endereco")]
    public string Endereco { get; set; } = string.Empty;

    [JsonPropertyName("telefone")]
    public string Telefone { get; set; } = string.Empty;

    [JsonPropertyName("cep")]
    public string Cep { get; set; } = string.Empty;

    [JsonPropertyName("municipio")]
    public string Municipio { get; set; } = string.Empty;

    [JsonPropertyName("matrizOuFilial")]
    public string MatrizOuFilial { get; set; } = string.Empty;

    [JsonPropertyName("numeroFuncionarios")]
    public int NumeroFuncionarios { get; set; }
}
