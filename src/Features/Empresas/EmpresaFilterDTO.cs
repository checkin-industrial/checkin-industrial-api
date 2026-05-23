using System.Text.Json.Serialization;

namespace AppTurismoIndustrial.Api.Features.Empresas;

/// <summary>
/// DTO de empresa filtrada para visualização de marcadores no mapa.
/// Mantém apenas os campos necessários para resposta rápida da API.
/// </summary>
public class EmpresaFilterDTO
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("nomeFantasia")]
    public string NomeFantasia { get; set; } = string.Empty;

    [JsonPropertyName("cnaePrincipal")]
    public string CnaePrincipal { get; set; } = string.Empty;

    [JsonPropertyName("descricaoCnae")]
    public string DescricaoCnae { get; set; } = string.Empty;

    [JsonPropertyName("endereco")]
    public string Endereco { get; set; } = string.Empty;

    [JsonPropertyName("setor")]
    public string Setor { get; set; } = string.Empty;

    [JsonPropertyName("porte")]
    public string Porte { get; set; } = string.Empty;

    [JsonPropertyName("telefone")]
    public string Telefone { get; set; } = string.Empty;

    [JsonPropertyName("cep")]
    public string Cep { get; set; } = string.Empty;

    [JsonPropertyName("municipio")]
    public string Municipio { get; set; } = string.Empty;

    [JsonPropertyName("matrizOuFilial")]
    public string MatrizOuFilial { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}
