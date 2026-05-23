namespace AppTurismoIndustrial.Api.Application.DTOs.Import;

/// <summary>
/// Resultado consolidado de uma operação de importação de empresas.
/// </summary>
public class EmpresaImportResult
{
    /// <summary>
    /// Total de registros lidos da fonte (CSV, JSON, API, etc).
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// Quantidade de empresas efetivamente inseridas no banco.
    /// </summary>
    public int Inserted { get; set; }

    /// <summary>
    /// Quantidade de empresas existentes que foram atualizadas no banco.
    /// </summary>
    public int Updated { get; set; }

    /// <summary>
    /// Quantidade de registros ignorados (duplicados, inválidos, já existentes).
    /// </summary>
    public int Skipped { get; set; }

    /// <summary>
    /// Lista detalhada de erros ocorridos durante a importação.
    /// </summary>
    public List<EmpresaImportError> Errors { get; set; } = [];

    /// <summary>
    /// Identificador único da sessão/execução de importação (para rastreabilidade).
    /// </summary>
    public string ImportSessionId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Data e hora do início da importação.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Data e hora do fim da importação.
    /// </summary>
    public DateTime CompletedAt { get; set; }

    /// <summary>
    /// Duração total da importação em milissegundos.
    /// </summary>
    public long DurationMs => (long)(CompletedAt - StartedAt).TotalMilliseconds;

    /// <summary>
    /// Fonte/origem da importação (CSV, JSON, SenaiApi, CnpjPublicApi, etc).
    /// </summary>
    public string ImportSource { get; set; } = string.Empty;

    /// <summary>
    /// Taxa de sucesso ((Inserted + Updated) / TotalRecords * 100).
    /// </summary>
    public double SuccessRate => TotalRecords > 0 ? ((Inserted + Updated) / (double)TotalRecords) * 100 : 0;

    /// <summary>
    /// Status da importação: Pending, InProgress, Completed, CompletedWithErrors, Failed.
    /// </summary>
    public string Status { get; set; } = "Completed";

    /// <summary>
    /// Mensagem adicional sobre a importação (motivo da falha, avisos, etc).
    /// </summary>
    public string? Message { get; set; }
}
