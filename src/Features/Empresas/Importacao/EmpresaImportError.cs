namespace AppTurismoIndustrial.Api.Features.Empresas.Importacao;

/// <summary>
/// Representa um erro ocorrido durante o processo de importação.
/// </summary>
public class EmpresaImportError
{
    /// <summary>
    /// Identificador do registro que gerou o erro (linha, índice ou ID da fonte).
    /// </summary>
    public string RecordId { get; set; } = string.Empty;

    /// <summary>
    /// CNPJ do registro (quando disponível).
    /// </summary>
    public string? Cnpj { get; set; }

    /// <summary>
    /// Tipo de erro: Validation, Format, Duplicate, GeocodingFailed, DatabaseError, etc.
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;

    /// <summary>
    /// Descrição detalhada do erro.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Campo específico que causou o erro (ex: "Cnpj", "Latitude", "Porte").
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Valor inválido que foi recebido, se aplicável (truncado para 500 caracteres).
    /// </summary>
    public string? InvalidValue { get; set; }

    /// <summary>
    /// Estágio onde o erro foi detectado: Parsing, Validation, Normalization, Geocoding, Persistence.
    /// </summary>
    public string Stage { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp do erro.
    /// </summary>
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
