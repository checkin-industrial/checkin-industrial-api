using AppTurismoIndustrial.Api.Shared.Validation;
using FluentValidation;
using AppValidationException = AppTurismoIndustrial.Api.Shared.Errors.ValidationException;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace AppTurismoIndustrial.Api.Tests.Shared.Validation;

public class ValidationFilterTests
{
    // DTO local + validator local pra isolar o teste de mudancas em DTOs de feature.
    private sealed class FakeDto
    {
        public string Nome { get; set; } = string.Empty;
    }

    private sealed class FakeDtoValidator : AbstractValidator<FakeDto>
    {
        public FakeDtoValidator()
        {
            RuleFor(d => d.Nome).NotEmpty().WithMessage("Nome obrigatorio.");
        }
    }

    [Fact]
    public async Task DtoValido_DelegaParaProximoFilter()
    {
        var filter = new ValidationFilter<FakeDto>(new FakeDtoValidator());
        var dto = new FakeDto { Nome = "ok" };

        var context = CreateContext(dto);
        var marker = new object();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(marker);

        var resultado = await filter.InvokeAsync(context, next);

        Assert.Same(marker, resultado);
    }

    [Fact]
    public async Task DtoInvalido_LancaValidationException()
    {
        var filter = new ValidationFilter<FakeDto>(new FakeDtoValidator());
        var dto = new FakeDto { Nome = string.Empty };

        var context = CreateContext(dto);
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(null);

        var exception = await Assert.ThrowsAsync<AppValidationException>(
            async () => await filter.InvokeAsync(context, next));

        Assert.Contains("Nome obrigatorio", exception.Message);
    }

    [Fact]
    public async Task SemDtoNosArgumentos_DelegaParaProximoFilter()
    {
        // Caso defensivo: se o filter for aplicado a um endpoint que nao recebe
        // o DTO esperado (ex: handler so com primitivos), deve passar adiante
        // sem validar nem falhar.
        var filter = new ValidationFilter<FakeDto>(new FakeDtoValidator());
        var context = CreateContext("nao-e-dto");
        var marker = new object();
        EndpointFilterDelegate next = _ => ValueTask.FromResult<object?>(marker);

        var resultado = await filter.InvokeAsync(context, next);

        Assert.Same(marker, resultado);
    }

    private static EndpointFilterInvocationContext CreateContext(params object[] arguments)
    {
        var httpContext = new DefaultHttpContext();
        var contextMock = new Mock<EndpointFilterInvocationContext>();
        contextMock.SetupGet(c => c.HttpContext).Returns(httpContext);
        contextMock.SetupGet(c => c.Arguments).Returns(arguments);
        return contextMock.Object;
    }
}
