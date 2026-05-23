
namespace AppTurismoIndustrial.Api.Features.Empresas.Importacao;

/// <summary>
/// Interface para parsers de importação de empresas de diferentes fontes.
/// </summary>
public interface IEmpresaImportParser
{
    /// <summary>
    /// Tipo de formato suportado pelo parser (CSV, JSON, API, etc).
    /// </summary>
    string FormatType { get; }

    /// <summary>
    /// Processa um fluxo de entrada e retorna registros de empresa em streaming.
    /// Ideal para arquivos grandes sem carregar tudo na memória.
    /// </summary>
    /// <param name="stream">Stream de entrada a processar.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Enumeração assíncrona de registros importados.</returns>
    IAsyncEnumerable<EmpresaImportRecord> ParseAsync(Stream stream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida se o format é compatível com este parser.
    /// </summary>
    /// <param name="contentType">Tipo MIME do formato.</param>
    /// <returns>True se o parser pode processar este tipo.</returns>
    bool SupportsFormat(string contentType);
}
