using System.Text.Json.Serialization;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public class DTOEmpresaVizinhancaResponse
{
    [JsonPropertyName("empresaBase")]
    public DTOEmpresaVizinhancaBase EmpresaBase { get; set; } = new();

    [JsonPropertyName("empresasProximas")]
    public List<DTOEmpresaVizinha> EmpresasProximas { get; set; } = [];
}

public class DTOEmpresaVizinhancaBase
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("nomeFantasia")]
    public string NomeFantasia { get; set; } = string.Empty;

    [JsonPropertyName("cnaePrincipal")]
    public string CnaePrincipal { get; set; } = string.Empty;

    [JsonPropertyName("setor")]
    public string Setor { get; set; } = string.Empty;

    [JsonPropertyName("numeroFuncionarios")]
    public int NumeroFuncionarios { get; set; }

    [JsonPropertyName("municipio")]
    public string Municipio { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}

public class DTOEmpresaVizinha
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("nomeFantasia")]
    public string NomeFantasia { get; set; } = string.Empty;

    [JsonPropertyName("cnaePrincipal")]
    public string CnaePrincipal { get; set; } = string.Empty;

    [JsonPropertyName("setor")]
    public string Setor { get; set; } = string.Empty;

    [JsonPropertyName("numeroFuncionarios")]
    public int NumeroFuncionarios { get; set; }

    [JsonPropertyName("municipio")]
    public string Municipio { get; set; } = string.Empty;

    [JsonPropertyName("distanciaMetros")]
    public double DistanciaMetros { get; set; }

    [JsonPropertyName("mesmoCnae")]
    public bool MesmoCnae { get; set; }

    [JsonPropertyName("mesmoSetor")]
    public bool MesmoSetor { get; set; }
}