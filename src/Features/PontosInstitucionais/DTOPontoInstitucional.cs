using System.Text.Json.Serialization;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public class DTOPontoInstitucional
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("nome")]
    public string Nome { get; set; } = string.Empty;

    [JsonPropertyName("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [JsonPropertyName("descricao")]
    public string Descricao { get; set; } = string.Empty;

    [JsonPropertyName("endereco")]
    public string Endereco { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("atividadesDisponiveis")]
    public string AtividadesDisponiveis { get; set; } = string.Empty;

    [JsonPropertyName("equipeGestao")]
    public string EquipeGestao { get; set; } = string.Empty;

    [JsonPropertyName("contatoNome")]
    public string ContatoNome { get; set; } = string.Empty;

    [JsonPropertyName("contatoTelefone")]
    public string ContatoTelefone { get; set; } = string.Empty;

    [JsonPropertyName("contatoEmail")]
    public string ContatoEmail { get; set; } = string.Empty;

    [JsonPropertyName("responsavelFotoUrl")]
    public string? ResponsavelFotoUrl { get; set; }

    [JsonPropertyName("logoUrl")]
    public string? LogoUrl { get; set; }

    [JsonPropertyName("cardFotoUrl")]
    public string? CardFotoUrl { get; set; }

    [JsonPropertyName("corMarcador")]
    public string CorMarcador { get; set; } = "#0d9488";

    [JsonPropertyName("iconeMarcador")]
    public string IconeMarcador { get; set; } = "institucional";

    [JsonPropertyName("ordemExibicao")]
    public int OrdemExibicao { get; set; }

    [JsonPropertyName("ativo")]
    public bool Ativo { get; set; }
}
