namespace AppTurismoIndustrial.Api.Application.Services;

/// <summary>
/// Resultado de uma geocodificação com latitude e longitude.
/// </summary>
public class GeocodeResult
{
    /// <summary>
    /// Latitude do endereço (-90 a 90).
    /// </summary>
    public decimal Latitude { get; set; }

    /// <summary>
    /// Longitude do endereço (-180 a 180).
    /// </summary>
    public decimal Longitude { get; set; }

    /// <summary>
    /// Precisão da geocodificação (exato, aproximado, cidade, etc).
    /// </summary>
    public string Accuracy { get; set; } = string.Empty;

    /// <summary>
    /// Provedor que realizou a geocodificação (Google, OpenStreetMap, etc).
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora em que o resultado foi obtido.
    /// </summary>
    public DateTime ObtainedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Interface para o serviço de geocodificação de endereços.
/// Converte endereços em coordenadas geográficas com cache de resultados.
/// </summary>
public interface IGeocodingService
{
    /// <summary>
    /// Geocodifica um endereço para latitude e longitude.
    /// Resultados são cacheados para evitar chamadas repetidas à API.
    /// </summary>
    /// <param name="endereco">Endereço completo a geocodificar.</param>
    /// <param name="cidade">Cidade do endereço (opcional, melhora precisão).</param>
    /// <param name="estado">Estado/UF do endereço (opcional).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>GeocodeResult com lat/lon se encontrado, null caso contrário.</returns>
    Task<GeocodeResult?> GeocodeAsync(
        string endereco,
        string? cidade = null,
        string? estado = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Limpa o cache de geocodificações.
    /// </summary>
    void ClearCache();

    /// <summary>
    /// Retorna o número de entradas em cache.
    /// </summary>
    int GetCacheSize();
}
