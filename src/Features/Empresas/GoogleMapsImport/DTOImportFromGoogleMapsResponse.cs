namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

/// <summary>
/// Response do POST /api/empresas/import/google-maps. Note que os contadores
/// agora se referem a CANDIDATES (entidade de triagem), nao mais a Empresas
/// criadas direto. O admin promove os candidates pra Empresa/Ponto/Telefone
/// via endpoints separados depois.
/// </summary>
public class DTOImportFromGoogleMapsResponse
{
    public Guid OperacaoId { get; set; }
    public int Encontrados { get; set; }

    /// <summary>Candidates novos criados a partir desta busca.</summary>
    public int CandidatesCriados { get; set; }

    /// <summary>
    /// Candidates pre-existentes (mesmo GooglePlaceId) que foram atualizados
    /// com dados mais frescos do Google (endereco, telefone, etc).
    /// </summary>
    public int CandidatesAtualizados { get; set; }

    /// <summary>Candidates pre-existentes sem mudanças relevantes nos dados.</summary>
    public int CandidatesIgnorados { get; set; }

    public IReadOnlyList<DTOImportResultItem> Itens { get; set; } = Array.Empty<DTOImportResultItem>();
}

public class DTOImportResultItem
{
    public string GooglePlaceId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;

    /// <summary>"criado" | "atualizado" | "ignorado"</summary>
    public string Acao { get; set; } = string.Empty;

    /// <summary>Id do candidate criado/atualizado/ignorado.</summary>
    public Guid? CandidateId { get; set; }

    public string? Motivo { get; set; }
}
