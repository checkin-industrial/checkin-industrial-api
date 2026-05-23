using System.ComponentModel.DataAnnotations;
using AppTurismoIndustrial.Api.Domain.Entities;

namespace AppTurismoIndustrial.Api.Application.DTOs;

public class DTOTelefoneUtilCriar
{
    [Required]
    [StringLength(180)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    public CategoriaTelefoneUtil Categoria { get; set; }

    [Required]
    [StringLength(80)]
    public string Telefone { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int? OrdemExibicao { get; set; }

    public bool? Ativo { get; set; }
}
