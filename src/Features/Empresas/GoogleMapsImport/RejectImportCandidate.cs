using Microsoft.AspNetCore.Http.HttpResults;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

public static class RejectImportCandidate
{
    public static RouteGroupBuilder MapRejectImportCandidate(this RouteGroupBuilder group)
    {
        group.MapPost("/{id:guid}/reject", Handle).WithName(nameof(RejectImportCandidate));
        return group;
    }

    /// <summary>
    /// Marca o destino como Rejeitado. Query param ?destino=empresa|ponto|telefone
    /// e obrigatorio — soft delete por destino, audit-friendly. Decisao terminal
    /// (nao pode reverter sem deletar a entidade-fim, se houver).
    /// </summary>
    private static async Task<Results<NoContent, BadRequest<object>>> Handle(
        Guid id,
        string destino,
        IImportCandidateService service,
        CancellationToken cancellationToken)
    {
        CandidateDestino? parsed = destino?.ToLowerInvariant() switch
        {
            "empresa" => CandidateDestino.Empresa,
            "ponto" => CandidateDestino.Ponto,
            "telefone" => CandidateDestino.Telefone,
            _ => null,
        };

        if (parsed is null)
        {
            return TypedResults.BadRequest<object>(new
            {
                erro = "Query param 'destino' obrigatorio. Valores: empresa, ponto, telefone."
            });
        }

        await service.RejectAsync(id, parsed.Value, cancellationToken);
        return TypedResults.NoContent();
    }
}
