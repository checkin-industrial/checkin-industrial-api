using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

/// <summary>
/// Status de promoção de um candidato para cada uma das 3 entidades-alvo
/// (Empresa, PontoInstitucional, TelefoneUtil). Decisão independente por destino:
/// o mesmo candidato pode estar Aprovado como Empresa, Rejeitado como Ponto e
/// ainda Pendente como Telefone simultaneamente.
/// </summary>
public enum CandidatePromotionStatus
{
    Pendente = 0,
    Aprovado = 1,
    Rejeitado = 2,
}

/// <summary>
/// Item bruto importado do Google Places que aguarda triagem do admin para
/// virar (ou não) Empresa, PontoInstitucional e/ou TelefoneUtil. Substituiu o
/// fluxo antigo onde imports criavam Empresas direto com Status=AguardandoRevisao
/// — agora a triagem é granular por destino.
/// </summary>
[Table("google_maps_import_candidates")]
public class GoogleMapsImportCandidate
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid GoogleMapsImportLogId { get; set; }
    public GoogleMapsImportLog? Log { get; set; }

    /// <summary>
    /// ID único do place no Google. Indexado: dedup ao re-importar o mesmo
    /// place no futuro (não cria candidato novo, atualiza dados se mudaram).
    /// </summary>
    [Required]
    [StringLength(200)]
    public string GooglePlaceId { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string? FormattedAddress { get; set; }

    [Range(-90, 90)]
    public decimal Latitude { get; set; }

    [Range(-180, 180)]
    public decimal Longitude { get; set; }

    [StringLength(50)]
    public string? Telefone { get; set; }

    /// <summary>
    /// Tipos do Google Places (array string) — informa o admin que tipo de
    /// destino faz sentido (ex: types=[hotel] → sugere PontoInstitucional;
    /// types=[manufacturer] → Empresa; sem types claros → Telefone).
    /// </summary>
    [Column(TypeName = "jsonb")]
    public string TypesJson { get; set; } = "[]";

    /// <summary>
    /// CEP de origem da busca (do GoogleMapsImportLog) — denormalizado pra
    /// facilitar o auto-fill no modal de promoção sem precisar JOIN.
    /// </summary>
    [StringLength(8)]
    public string? CepOrigem { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // ─── Decisão de promoção para Empresa ─────────────────────────────────
    public CandidatePromotionStatus EmpresaStatus { get; set; } = CandidatePromotionStatus.Pendente;
    public Guid? EmpresaId { get; set; }
    public DateTime? EmpresaDecididoEm { get; set; }

    // ─── Decisão de promoção para PontoInstitucional ──────────────────────
    public CandidatePromotionStatus PontoStatus { get; set; } = CandidatePromotionStatus.Pendente;
    public Guid? PontoInstitucionalId { get; set; }
    public DateTime? PontoDecididoEm { get; set; }

    // ─── Decisão de promoção para TelefoneUtil ────────────────────────────
    public CandidatePromotionStatus TelefoneStatus { get; set; } = CandidatePromotionStatus.Pendente;
    public Guid? TelefoneUtilId { get; set; }
    public DateTime? TelefoneDecididoEm { get; set; }
}
