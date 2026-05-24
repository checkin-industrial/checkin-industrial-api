using FluentValidation;
using AppValidationException = AppTurismoIndustrial.Api.Shared.Errors.ValidationException;

namespace AppTurismoIndustrial.Api.Shared.Validation;

/// <summary>
/// Endpoint filter generico que valida o argumento tipado &lt;T&gt; usando um
/// IValidator&lt;T&gt; resolvido do DI. Falhas viram ValidationException - convertida
/// em 400 ProblemDetails pelo ProblemDetailsMiddleware.
///
/// Uso: <c>.AddEndpointFilter&lt;ValidationFilter&lt;DTOEmpresaCriar&gt;&gt;()</c> no MapPost/MapPut.
/// O filter procura o primeiro argumento do tipo &lt;T&gt; nos parametros do handler.
/// </summary>
public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var dto = context.Arguments.OfType<T>().FirstOrDefault();
        if (dto is null)
        {
            return await next(context);
        }

        var result = await _validator.ValidateAsync(dto, context.HttpContext.RequestAborted);
        if (!result.IsValid)
        {
            var message = string.Join("; ", result.Errors.Select(e => e.ErrorMessage));
            throw new AppValidationException(message);
        }

        return await next(context);
    }
}
