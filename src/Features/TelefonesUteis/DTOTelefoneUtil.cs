using System.Text.Json.Serialization;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public class DTOTelefoneUtil
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("categoria")]
    public string Categoria { get; set; } = string.Empty;

    [JsonPropertyName("telefone")]
    public string Telefone { get; set; } = string.Empty;

    [JsonPropertyName("ordemExibicao")]
    public int OrdemExibicao { get; set; }

    [JsonPropertyName("ativo")]
    public bool Ativo { get; set; }
}
