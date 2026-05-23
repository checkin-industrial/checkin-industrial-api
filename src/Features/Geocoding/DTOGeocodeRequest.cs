using System.ComponentModel.DataAnnotations;

namespace AppTurismoIndustrial.Api.Features.Geocoding;

public class DTOGeocodeRequest
{
    [Required]
    [StringLength(300)]
    public string Endereco { get; set; } = string.Empty;

    [StringLength(150)]
    public string? Municipio { get; set; }

    [StringLength(2)]
    public string? Estado { get; set; }
}
