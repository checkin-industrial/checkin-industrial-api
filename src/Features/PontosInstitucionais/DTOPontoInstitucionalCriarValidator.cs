using FluentValidation;

namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public sealed class DTOPontoInstitucionalCriarValidator : AbstractValidator<DTOPontoInstitucionalCriar>
{
    public DTOPontoInstitucionalCriarValidator()
    {
        RuleFor(p => p.Nome)
            .NotEmpty().WithMessage("Nome e obrigatorio.")
            .MaximumLength(180);

        RuleFor(p => p.Tipo)
            .IsInEnum().WithMessage("Tipo de ponto institucional invalido.");

        RuleFor(p => p.Descricao)
            .NotEmpty().WithMessage("Descricao e obrigatoria.")
            .MaximumLength(400);

        RuleFor(p => p.Endereco)
            .NotEmpty().WithMessage("Endereco e obrigatorio.")
            .MaximumLength(300);

        RuleFor(p => p.Latitude)
            .InclusiveBetween(-90m, 90m).WithMessage("Latitude deve estar entre -90 e 90.");

        RuleFor(p => p.Longitude)
            .InclusiveBetween(-180m, 180m).WithMessage("Longitude deve estar entre -180 e 180.");

        RuleFor(p => p.AtividadesDisponiveis).MaximumLength(300);
        RuleFor(p => p.EquipeGestao).MaximumLength(250);
        RuleFor(p => p.ContatoNome).MaximumLength(180);
        RuleFor(p => p.ContatoTelefone).MaximumLength(20);

        RuleFor(p => p.ContatoEmail!)
            .EmailAddress().When(p => !string.IsNullOrEmpty(p.ContatoEmail))
            .WithMessage("Email de contato invalido.")
            .MaximumLength(150);

        RuleFor(p => p.ResponsavelFotoUrl).MaximumLength(500);
        RuleFor(p => p.LogoUrl).MaximumLength(500);
        RuleFor(p => p.CardFotoUrl).MaximumLength(500);
        RuleFor(p => p.CorMarcador).MaximumLength(20);
        RuleFor(p => p.IconeMarcador).MaximumLength(60);

        RuleFor(p => p.OrdemExibicao!)
            .GreaterThanOrEqualTo(0).When(p => p.OrdemExibicao.HasValue);
    }
}
