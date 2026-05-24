using FluentValidation;

namespace AppTurismoIndustrial.Api.Features.Empresas;

/// <summary>
/// Validator FluentValidation para DTOEmpresaAtualizar. Mesmas regras de
/// DTOEmpresaCriarValidator - duplicacao consciente (DTOs sao tipos distintos
/// no contrato OpenAPI, manter separados evita acoplar mensagens cross-DTO).
/// </summary>
public sealed class DTOEmpresaAtualizarValidator : AbstractValidator<DTOEmpresaAtualizar>
{
    public DTOEmpresaAtualizarValidator()
    {
        RuleFor(e => e.Cnpj)
            .NotEmpty().WithMessage("CNPJ e obrigatorio.")
            .Matches(@"^\d{14}$").WithMessage("CNPJ deve conter exatamente 14 digitos numericos.");

        RuleFor(e => e.RazaoSocial)
            .NotEmpty().WithMessage("Razao social e obrigatoria.")
            .MaximumLength(200);

        RuleFor(e => e.NomeFantasia)
            .NotEmpty().WithMessage("Nome fantasia e obrigatorio.")
            .MaximumLength(200);

        RuleFor(e => e.CnaePrincipal)
            .NotEmpty().WithMessage("CNAE principal e obrigatorio.")
            .Matches(@"^\d{7}$").WithMessage("CNAE principal deve conter 7 digitos numericos.");

        RuleFor(e => e.Setor)
            .IsInEnum().WithMessage("Setor invalido.");

        RuleFor(e => e.Porte)
            .IsInEnum().WithMessage("Porte invalido.");

        RuleFor(e => e.MatrizOuFilial)
            .IsInEnum().WithMessage("MatrizOuFilial invalido.");

        RuleFor(e => e.SituacaoCadastral)
            .IsInEnum().WithMessage("Situacao cadastral invalida.");

        RuleFor(e => e.Status!)
            .IsInEnum().When(e => e.Status.HasValue).WithMessage("Status invalido.");

        RuleFor(e => e.NumeroFuncionarios!)
            .GreaterThanOrEqualTo(0).When(e => e.NumeroFuncionarios.HasValue);

        RuleFor(e => e.Endereco)
            .NotEmpty().WithMessage("Endereco e obrigatorio.")
            .MaximumLength(300);

        RuleFor(e => e.Telefone)
            .MaximumLength(20);

        RuleFor(e => e.Cep!)
            .Matches(@"^\d{8}$").When(e => !string.IsNullOrEmpty(e.Cep))
            .WithMessage("CEP deve conter exatamente 8 digitos numericos.");

        RuleFor(e => e.Municipio)
            .NotEmpty().WithMessage("Municipio e obrigatorio.")
            .MaximumLength(150);

        RuleFor(e => e.DescricaoCnae)
            .NotEmpty().WithMessage("Descricao do CNAE e obrigatoria.")
            .MaximumLength(300);

        RuleFor(e => e.Latitude)
            .InclusiveBetween(-90m, 90m).WithMessage("Latitude deve estar entre -90 e 90.");

        RuleFor(e => e.Longitude)
            .InclusiveBetween(-180m, 180m).WithMessage("Longitude deve estar entre -180 e 180.");
    }
}
