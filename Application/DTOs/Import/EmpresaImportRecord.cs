namespace AppTurismoIndustrial.Api.Application.DTOs.Import;

/// <summary>
/// Representa um registro bruto de empresa importado de uma fonte externa.
/// </summary>
public class EmpresaImportRecord
{
    /// <summary>
    /// Identificador único do registro na importação (linha CSV, índice JSON, etc).
    /// </summary>
    public string RecordId { get; set; } = string.Empty;

    /// <summary>
    /// CNPJ da empresa (14 caracteres numéricos).
    /// </summary>
    public string Cnpj { get; set; } = string.Empty;

    /// <summary>
    /// Razão social da empresa.
    /// </summary>
    public string RazaoSocial { get; set; } = string.Empty;

    /// <summary>
    /// Nome fantasia da empresa.
    /// </summary>
    public string NomeFantasia { get; set; } = string.Empty;

    /// <summary>
    /// CNAE principal (código de atividade).
    /// </summary>
    public string CnaePrincipal { get; set; } = string.Empty;

    /// <summary>
    /// Setor: Industria, Comercio, Servicos.
    /// </summary>
    public string Setor { get; set; } = string.Empty;

    /// <summary>
    /// Porte: MEI, ME, EPP, LTDA, SA.
    /// </summary>
    public string Porte { get; set; } = string.Empty;

    /// <summary>
    /// Número de funcionários.
    /// </summary>
    public int? NumeroFuncionarios { get; set; }

    /// <summary>
    /// Endereço da empresa.
    /// </summary>
    public string Endereco { get; set; } = string.Empty;

    /// <summary>
    /// Telefone principal da empresa.
    /// </summary>
    public string Telefone { get; set; } = string.Empty;

    /// <summary>
    /// CEP da empresa (8 dígitos).
    /// </summary>
    public string Cep { get; set; } = string.Empty;

    /// <summary>
    /// Município da empresa.
    /// </summary>
    public string Municipio { get; set; } = string.Empty;

    /// <summary>
    /// Descrição textual da atividade CNAE principal.
    /// </summary>
    public string DescricaoCnae { get; set; } = string.Empty;

    /// <summary>
    /// Tipo da unidade empresarial: Matriz ou Filial.
    /// </summary>
    public string MatrizOuFilial { get; set; } = string.Empty;

    /// <summary>
    /// Latitude geográfica (pode estar vazia se requer geocodificação).
    /// </summary>
    public decimal? Latitude { get; set; }

    /// <summary>
    /// Longitude geográfica (pode estar vazia se requer geocodificação).
    /// </summary>
    public decimal? Longitude { get; set; }

    /// <summary>
    /// Situação cadastral: Ativa, Inativa, Suspensa, Baixada.
    /// </summary>
    public string SituacaoCadastral { get; set; } = string.Empty;

    /// <summary>
    /// Data da importação (opcional, pode ser preenchida pela fonte).
    /// </summary>
    public DateTime? DataImportacao { get; set; }

    /// <summary>
    /// Fonte da importação para rastreabilidade e auditoria.
    /// </summary>
    public string FonteOrigem { get; set; } = string.Empty;
}
