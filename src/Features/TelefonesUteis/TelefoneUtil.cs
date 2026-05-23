using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

[Table("telefones_uteis")]
public class TelefoneUtil
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(180)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public CategoriaTelefoneUtil Categoria { get; set; }

    [Required]
    [StringLength(80)]
    public string Telefone { get; set; } = string.Empty;

    public int? OrdemExibicao { get; set; }

    public bool? Ativo { get; set; }
}

public enum CategoriaTelefoneUtil
{
    EmergenciaServicosPublicos = 1,
    TransporteCultura = 2,
    HoteisPousadas = 3,
}
