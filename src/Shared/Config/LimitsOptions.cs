using System.ComponentModel.DataAnnotations;

namespace AppTurismoIndustrial.Api.Shared.Config;

/// <summary>
/// Limites operacionais da API (caps de paginacao, batch sizes, etc.).
/// Bindado de `appsettings.json:Limits`. Services consomem via `IOptions&lt;LimitsOptions&gt;`.
/// </summary>
/// <remarks>
/// Defaults aqui replicam as constantes historicas dos services. Se a secao `Limits`
/// nao existir na config, o binder usa esses valores (comportamento identico ao codigo
/// antes da extracao). Services tambem aceitam <c>IOptions&lt;LimitsOptions&gt;? = null</c>
/// para compat com testes que instanciam o service direto sem DI.
/// </remarks>
public sealed class LimitsOptions
{
    /// <summary>Cap de registros retornados pelo endpoint de mapa de empresas.</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int MaxMapRecords { get; set; } = 5000;

    /// <summary>Cap de registros retornados pelo endpoint de filtro de empresas.</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int MaxFilterRecords { get; set; } = 10000;

    /// <summary>Cap de pontos enviados ao frontend pelo heatmap industrial.</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int MaxPointsForFrontend { get; set; } = 5000;

    /// <summary>Tamanho do lote usado pela importacao de empresas (CSV/JSON).</summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int DefaultBatchSize { get; set; } = 1000;
}
