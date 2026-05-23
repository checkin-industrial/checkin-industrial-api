using System;
using System.ComponentModel.DataAnnotations;
using AppTurismoIndustrial.Api.Domain.Entities;

namespace AppTurismoIndustrial.Api.Application.DTOs;

public class DTOEmpresaAtualizar
{
    [Required]
    [StringLength(14, MinimumLength = 14)]
    [RegularExpression(@"^\d{14}$", ErrorMessage = "CNPJ deve conter exatamente 14 digitos numericos.")]
    public string Cnpj { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string RazaoSocial { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string NomeFantasia { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [RegularExpression(@"^\d{7}$", ErrorMessage = "CNAE principal deve conter 7 digitos numericos.")]
    public string CnaePrincipal { get; set; } = string.Empty;

    [Required]
    public SetorEmpresa Setor { get; set; }

    [Required]
    public PorteEmpresa Porte { get; set; }

    [Range(0, int.MaxValue)]
    public int? NumeroFuncionarios { get; set; }

    [Required]
    [StringLength(300)]
    public string Endereco { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefone { get; set; }

    [StringLength(8, MinimumLength = 8)]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "CEP deve conter exatamente 8 digitos numericos.")]
    public string? Cep { get; set; }

    [Required]
    [StringLength(150)]
    public string Municipio { get; set; } = string.Empty;

    [Required]
    [StringLength(300)]
    public string DescricaoCnae { get; set; } = string.Empty;

    [Required]
    public MatrizOuFilialEmpresa MatrizOuFilial { get; set; }

    [Range(-90d, 90d, ErrorMessage = "Latitude deve estar entre -90 e 90.")]
    public decimal Latitude { get; set; }

    [Range(-180d, 180d, ErrorMessage = "Longitude deve estar entre -180 e 180.")]
    public decimal Longitude { get; set; }

    [Required]
    public SituacaoCadastral SituacaoCadastral { get; set; }

    public DateTime? DataCadastro { get; set; }
}
