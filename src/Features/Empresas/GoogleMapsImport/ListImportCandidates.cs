using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

public static class ListImportCandidates
{
    public static RouteGroupBuilder MapListImportCandidates(this RouteGroupBuilder group)
    {
        group.MapGet("/", Handle).WithName(nameof(ListImportCandidates));
        return group;
    }

    /// <summary>
    /// Lista candidates ordenados por mais recente. Filtro opcional ?status=pendente|aprovado|rejeitado
    /// casa contra QUALQUER um dos 3 destinos (admin tipicamente busca "tem destino
    /// pendente"). Sem filtro retorna tudo.
    /// </summary>
    private static async Task<Ok<IReadOnlyList<DTOImportCandidateResponse>>> Handle(
        IImportCandidateService service,
        CancellationToken cancellationToken,
        string? status = null)
    {
        CandidatePromotionStatus? statusFiltro = status?.ToLowerInvariant() switch
        {
            "pendente" => CandidatePromotionStatus.Pendente,
            "aprovado" => CandidatePromotionStatus.Aprovado,
            "rejeitado" => CandidatePromotionStatus.Rejeitado,
            _ => null,
        };

        var candidates = await service.ListAsync(statusFiltro, cancellationToken);
        return TypedResults.Ok(candidates);
    }
}
