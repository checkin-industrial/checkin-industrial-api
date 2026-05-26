namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

public interface IImportCandidateService
{
    Task<IReadOnlyList<DTOImportCandidateResponse>> ListAsync(
        CandidatePromotionStatus? filtroStatusQualquerDestino,
        CancellationToken cancellationToken = default);

    Task<DTOImportCandidateResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cria uma Empresa Ativo a partir do DTO e marca o candidato como Aprovado
    /// pro destino Empresa. Reusa EmpresaService.CriarAsync (valida CNPJ unique,
    /// etc). Lanca ConflictException se candidato ja aprovado/rejeitado pra Empresa.
    /// </summary>
    Task<DTORespostaEmpresa> PromoteToEmpresaAsync(
        Guid candidateId, DTOEmpresaCriar dto, CancellationToken cancellationToken = default);

    Task<DTOPontoInstitucional> PromoteToPontoAsync(
        Guid candidateId, DTOPontoInstitucionalCriar dto, CancellationToken cancellationToken = default);

    Task<DTOTelefoneUtil> PromoteToTelefoneAsync(
        Guid candidateId, DTOTelefoneUtilCriar dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca o destino como Rejeitado. Diferente das promocoes, rejeitar nao cria
    /// nenhuma entidade-fim — apenas anota a decisao no candidato (soft, auditavel).
    /// </summary>
    Task RejectAsync(Guid candidateId, CandidateDestino destino, CancellationToken cancellationToken = default);
}
