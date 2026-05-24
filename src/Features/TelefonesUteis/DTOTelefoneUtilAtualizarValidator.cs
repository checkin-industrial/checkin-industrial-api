using FluentValidation;

namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public sealed class DTOTelefoneUtilAtualizarValidator : AbstractValidator<DTOTelefoneUtilAtualizar>
{
    public DTOTelefoneUtilAtualizarValidator()
    {
        RuleFor(t => t.Nome)
            .NotEmpty().WithMessage("Nome e obrigatorio.")
            .MaximumLength(180);

        RuleFor(t => t.Categoria)
            .IsInEnum().WithMessage("Categoria invalida.");

        RuleFor(t => t.Telefone)
            .NotEmpty().WithMessage("Telefone e obrigatorio.")
            .MaximumLength(80);

        RuleFor(t => t.OrdemExibicao!)
            .GreaterThanOrEqualTo(0).When(t => t.OrdemExibicao.HasValue);
    }
}
