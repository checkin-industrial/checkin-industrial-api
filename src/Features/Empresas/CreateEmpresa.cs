using AppTurismoIndustrial.Api.Shared.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public static class CreateEmpresa
{
    public static RouteHandlerBuilder MapCreateEmpresa(this RouteGroupBuilder group)
    {
        return group.MapPost("/", Handle)
            .WithName(nameof(CreateEmpresa))
            .AddEndpointFilter<ValidationFilter<DTOEmpresaCriar>>();
    }

    private static async Task<Created<DTORespostaEmpresa>> Handle(
        DTOEmpresaCriar dto,
        IEmpresaService service,
        CancellationToken cancellationToken)
    {
        // 409 (CNPJ duplicado) via ConflictException -> ProblemDetailsMiddleware.
        var empresa = await service.CriarAsync(dto, cancellationToken);
        return TypedResults.Created($"/api/empresas/{empresa.Id}", empresa);
    }
}
