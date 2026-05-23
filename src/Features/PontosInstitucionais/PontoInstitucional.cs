using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

[Table("pontos_institucionais")]
public class PontoInstitucional
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(180)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public TipoPontoInstitucional Tipo { get; set; }

    [Required]
    [StringLength(400)]
    public string Descricao { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string Endereco { get; set; } = string.Empty;

    [Precision(9, 6)]
    [Range(-90d, 90d, ErrorMessage = "Latitude deve estar entre -90 e 90.")]
    public decimal Latitude { get; set; }

    [Precision(9, 6)]
    [Range(-180d, 180d, ErrorMessage = "Longitude deve estar entre -180 e 180.")]
    public decimal Longitude { get; set; }

    [StringLength(300)]
    public string? AtividadesDisponiveis { get; set; }

    [StringLength(250)]
    public string? EquipeGestao { get; set; }

    [StringLength(180)]
    public string? ContatoNome { get; set; }

    [StringLength(20)]
    public string? ContatoTelefone { get; set; }

    [StringLength(150)]
    public string? ContatoEmail { get; set; }

    [StringLength(500)]
    public string? ResponsavelFotoUrl { get; set; }

    [StringLength(500)]
    public string? LogoUrl { get; set; }

    [StringLength(500)]
    public string? CardFotoUrl { get; set; }

    [StringLength(20)]
    public string? CorMarcador { get; set; }

    [StringLength(60)]
    public string? IconeMarcador { get; set; }

    public int? OrdemExibicao { get; set; }

    public bool? Ativo { get; set; }
}

public enum TipoPontoInstitucional
{
    Educacao = 1,
    Comercio = 2,
    Financeiro = 3,
    Servico = 4,
    SetorPrefeitura = 5,
    PontoTuristico = 6,
    Hotel = 7,
    Ecoturismo = 8
}
