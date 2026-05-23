using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public static class CreateEmpresa
{
    public static RouteHandlerBuilder MapCreateEmpresa(this RouteGroupBuilder group)
    {
        return group.MapPost("/", Handle)
            .WithName(nameof(CreateEmpresa));
    }

    private static async Task<Results<Created<DTORespostaEmpresa>, Conflict<object>>> Handle(
        DTOEmpresaCriar dto,
        IEmpresaService service)
    {
        var (empresa, cnpjDuplicado) = await service.CriarAsync(dto);

        if (cnpjDuplicado)
        {
            return TypedResults.Conflict<object>(new { message = "Ja existe uma empresa cadastrada com este CNPJ." });
        }

        return TypedResults.Created($"/api/empresas/{empresa!.Id}", empresa);
    }
}
