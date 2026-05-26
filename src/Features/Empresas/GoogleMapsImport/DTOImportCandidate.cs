namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

/// <summary>
/// Snapshot de um candidato pra UI de triagem. Inclui dados originais do Google +
/// o estado de decisao por destino. Types vem como array (deserializado do
/// TypesJson) pra facilitar consumo no painel.
///
/// Promocoes (POST /candidates/{id}/promote-*) aceitam os DTOs Criar de cada
/// feature direto no body (DTOEmpresaCriar, DTOPontoInstitucionalCriar,
/// DTOTelefoneUtilCriar) — reusa validators + unique constraints existentes
/// sem duplicar.
/// </summary>
public class DTOImportCandidateResponse
{
    public Guid Id { get; set; }
    public string GooglePlaceId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string? FormattedAddress { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? Telefone { get; set; }
    public IReadOnlyList<string> Types { get; set; } = Array.Empty<string>();
    public string? CepOrigem { get; set; }
    public DateTime CriadoEm { get; set; }

    public CandidatePromotionStatus EmpresaStatus { get; set; }
    public Guid? EmpresaId { get; set; }
    public DateTime? EmpresaDecididoEm { get; set; }

    public CandidatePromotionStatus PontoStatus { get; set; }
    public Guid? PontoInstitucionalId { get; set; }
    public DateTime? PontoDecididoEm { get; set; }

    public CandidatePromotionStatus TelefoneStatus { get; set; }
    public Guid? TelefoneUtilId { get; set; }
    public DateTime? TelefoneDecididoEm { get; set; }
}

/// <summary>
/// Identifica qual dos 3 destinos esta sendo rejeitado/consultado.
/// </summary>
public enum CandidateDestino
{
    Empresa = 1,
    Ponto = 2,
    Telefone = 3,
}
