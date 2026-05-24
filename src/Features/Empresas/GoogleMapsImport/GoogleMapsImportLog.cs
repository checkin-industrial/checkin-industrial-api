using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

// Log persistente de cada operacao de import do Google Maps. Guarda request + response
// raw em jsonb para audit e re-processamento futuro (re-enriquecer cadastros sem
// pagar nova chamada). Cada operacao agrupa multiplas chamadas (geocode + nearby search
// + opcionalmente place details).
[Table("google_maps_import_log")]
public class GoogleMapsImportLog
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(8)]
    public string Cep { get; set; } = string.Empty;

    [Required]
    public int RaioMetros { get; set; }

    [Required]
    [StringLength(50)]
    public string Tipo { get; set; } = string.Empty;

    public decimal LatitudeOrigem { get; set; }
    public decimal LongitudeOrigem { get; set; }

    // JSON da resposta completa do Google Places. Persistido pra:
    // 1) auditar custos e padroes de uso
    // 2) re-enriquecer empresas existentes em runs futuros (sem custo extra)
    [Column(TypeName = "jsonb")]
    public string ResponseRaw { get; set; } = "{}";

    public int EmpresasCriadas { get; set; }
    public int EmpresasAtualizadas { get; set; }
    public int EmpresasIgnoradas { get; set; }

    [StringLength(2000)]
    public string? Erro { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}
