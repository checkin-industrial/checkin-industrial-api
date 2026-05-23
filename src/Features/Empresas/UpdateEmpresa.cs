using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public static class UpdateEmpresa
{
    public static RouteGroupBuilder MapUpdateEmpresa(this RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", Handle)
            .WithName(nameof(UpdateEmpresa));
        return group;
    }

    private static async Task<Results<NoContent, BadRequest, NotFound, Conflict<object>>> Handle(
        Guid id,
        DTOEmpresaAtualizar dto,
        IEmpresaService service)
    {
        var (atualizado, naoEncontrada, cnpjDuplicado) = await service.AtualizarAsync(id, dto);

        if (naoEncontrada)
        {
            return TypedResults.NotFound();
        }

        if (cnpjDuplicado)
        {
            return TypedResults.Conflict<object>(new { message = "Ja existe uma empresa cadastrada com este CNPJ." });
        }

        if (!atualizado)
        {
            return TypedResults.BadRequest();
        }

        return TypedResults.NoContent();
    }
}
