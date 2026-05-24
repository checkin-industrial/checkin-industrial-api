namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

public class DTOImportFromGoogleMapsResponse
{
    public Guid OperacaoId { get; set; }
    public int Encontrados { get; set; }
    public int Criados { get; set; }
    public int Atualizados { get; set; }
    public int Ignorados { get; set; }
    public IReadOnlyList<DTOImportResultItem> Itens { get; set; } = Array.Empty<DTOImportResultItem>();
}

public class DTOImportResultItem
{
    public string GooglePlaceId { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty; // "criado" | "atualizado" | "ignorado"
    public Guid? EmpresaId { get; set; }
    public string? Motivo { get; set; }
}
