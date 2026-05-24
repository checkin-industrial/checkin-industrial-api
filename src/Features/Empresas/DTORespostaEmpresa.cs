using System;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public class DTORespostaEmpresa
{
    public Guid Id { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public string CnaePrincipal { get; set; } = string.Empty;
    public string DescricaoCnae { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public string Municipio { get; set; } = string.Empty;
    public string MatrizOuFilial { get; set; } = string.Empty;
    public MatrizOuFilialEmpresa MatrizOuFilialCodigo { get; set; }
    public SetorEmpresa Setor { get; set; }
    public PorteEmpresa Porte { get; set; }
    public int NumeroFuncionarios { get; set; }
    public string Endereco { get; set; } = string.Empty;
    public SituacaoCadastral SituacaoCadastral { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
    public StatusEmpresa Status { get; set; } = StatusEmpresa.Ativo;
}
