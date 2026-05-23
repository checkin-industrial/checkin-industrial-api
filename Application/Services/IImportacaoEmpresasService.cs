using AppTurismoIndustrial.Api.Application.DTOs.Import;

namespace AppTurismoIndustrial.Api.Application.Services;

/// <summary>
/// Interface para o serviço de importação de empresas.
/// Orquestra todo o fluxo de parse, validação, deduplicação e persistência.
/// </summary>
public interface IImportacaoEmpresasService
{
    /// <summary>
    /// Importa empresas de um stream em um formato específico.
    /// </summary>
    /// <param name="stream">Stream contendo os dados a importar.</param>
    /// <param name="formato">Formato do stream: CSV, JSON, API, etc.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado consolidado da importação com estatísticas e erros.</returns>
    Task<EmpresaImportResult> ImportarAsync(
        Stream stream,
        string formato,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Importa registros já parseados (para integração com diferentes parsers).
    /// </summary>
    /// <param name="registros">Enumeração assíncrona de registros a importar.</param>
    /// <param name="nomeOrigem">Nome da fonte/origem para rastreabilidade.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado consolidado da importação.</returns>
    Task<EmpresaImportResult> ImportarRegistrosAsync(
        IAsyncEnumerable<EmpresaImportRecord> registros,
        string nomeOrigem,
        CancellationToken cancellationToken = default);
}
