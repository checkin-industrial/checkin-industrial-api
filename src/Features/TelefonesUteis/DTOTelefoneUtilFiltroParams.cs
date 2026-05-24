namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public class DTOTelefoneUtilFiltroParams
{
    public string? Categoria { get; set; }

    // "true"/"false"/"todos"/null. Parsing via FilterHelpers.ParseAtivo.
    // String tri-state (em vez de bool?) converge com o padrao do painel
    // (dropdown "Todas/Ativos/Inativos" pode passar "todos" explicito).
    public string? Ativo { get; set; }

    public string? Termo { get; set; }
}
