namespace AppTurismoIndustrial.Api.Shared.Errors;

/// <summary>
/// Excecao base do dominio mapeada para ProblemDetails pelo middleware global.
/// Subclasses devem definir o StatusCode HTTP apropriado.
/// </summary>
public abstract class AppException : Exception
{
    public abstract int StatusCode { get; }

    protected AppException(string message) : base(message) { }

    protected AppException(string message, Exception inner) : base(message, inner) { }
}

public sealed class NotFoundException : AppException
{
    public override int StatusCode => StatusCodes.Status404NotFound;
    public NotFoundException(string message) : base(message) { }
}

public sealed class ValidationException : AppException
{
    public override int StatusCode => StatusCodes.Status400BadRequest;
    public ValidationException(string message) : base(message) { }
}

public sealed class ConflictException : AppException
{
    public override int StatusCode => StatusCodes.Status409Conflict;
    public ConflictException(string message) : base(message) { }
}
