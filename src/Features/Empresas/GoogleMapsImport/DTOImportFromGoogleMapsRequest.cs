using System.ComponentModel.DataAnnotations;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

public class DTOImportFromGoogleMapsRequest
{
    [Required]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "CEP deve conter exatamente 8 digitos numericos.")]
    public string Cep { get; set; } = string.Empty;

    [Required]
    [Range(100, 50_000, ErrorMessage = "Raio deve estar entre 100 e 50000 metros.")]
    public int RaioMetros { get; set; }

    // Slug de GooglePlaceTypeMapping. Ex: "industria", "loja", "farmacia".
    [Required]
    [StringLength(50)]
    public string Tipo { get; set; } = string.Empty;
}
