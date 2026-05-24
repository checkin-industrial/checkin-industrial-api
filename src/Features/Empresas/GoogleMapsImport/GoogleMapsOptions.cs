using System.ComponentModel.DataAnnotations;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

// Options pattern para configurar a integracao com Google Maps Platform.
// Bind em Program.cs via .Configure<GoogleMapsOptions>(config.GetSection("GoogleMaps"));
//
// CONFIG (env vars com __ entre niveis):
//   GoogleMaps__ApiKey=AIza...                  (NUNCA commitada; passa por secret manager)
//   GoogleMaps__PlacesBaseUrl=https://places.googleapis.com/v1/
//   GoogleMaps__MaxRaioMetros=10000
//   GoogleMaps__AllowedRegion__LatMin=-23.0     (opcional: bounds da regiao da massa de dados)
//   GoogleMaps__AllowedRegion__LatMax=-22.0
//   GoogleMaps__AllowedRegion__LngMin=-50.0
//   GoogleMaps__AllowedRegion__LngMax=-48.0
public class GoogleMapsOptions
{
    public const string SectionName = "GoogleMaps";

    public string ApiKey { get; set; } = string.Empty;

    public string PlacesBaseUrl { get; set; } = "https://places.googleapis.com/v1/";

    // Cap absoluto. O endpoint rejeita raios maiores como BadRequest.
    [Range(100, 50_000)]
    public int MaxRaioMetros { get; set; } = 10_000;

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
