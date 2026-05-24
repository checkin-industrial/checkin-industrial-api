using System.ComponentModel.DataAnnotations;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

// Options pattern para configurar a integracao com Google Maps Platform.
// Bind em Program.cs via .Configure<GoogleMapsOptions>(config.GetSection("GoogleMaps"));
//
// CONFIG (env vars com __ entre niveis):
//   GoogleMaps__ApiKey=AIza...                  (NUNCA commitada; passa por secret manager)
//   GoogleMaps__PlacesBaseUrl=https://places.googleapis.com/v1/
//   GoogleMaps__MaxRaioMetros=1000              (default conservador: 1 km por busca)
//   GoogleMaps__AllowedRegion__LatMin=-22.3197  (default ~1 km box centrado em Bauru/SP -
//   GoogleMaps__AllowedRegion__LatMax=-22.3097   ajustar para a regiao real da massa de dados
//   GoogleMaps__AllowedRegion__LngMin=-49.0656   antes do primeiro uso em prod)
//   GoogleMaps__AllowedRegion__LngMax=-49.0556
public class GoogleMapsOptions
{
    public const string SectionName = "GoogleMaps";

    public string ApiKey { get; set; } = string.Empty;

    public string PlacesBaseUrl { get; set; } = "https://places.googleapis.com/v1/";

    // Cap absoluto. O endpoint rejeita raios maiores como BadRequest.
    // Default conservador: 1 km. Owner pode aumentar via env var quando validado.
    [Range(100, 50_000)]
    public int MaxRaioMetros { get; set; } = 1_000;

    // Bounding box opcional. Quando configurada, a geocodificacao do CEP precisa
    // cair dentro da box; do contrario o request e rejeitado (protege contra
    // gastos acidentais fora da regiao prevista).
    public RegionBounds? AllowedRegion { get; set; }
}

public class RegionBounds
{
    public double LatMin { get; set; }
    public double LatMax { get; set; }
    public double LngMin { get; set; }
    public double LngMax { get; set; }

    public bool Contains(double lat, double lng)
    {
        return lat >= LatMin && lat <= LatMax && lng >= LngMin && lng <= LngMax;
    }
}
