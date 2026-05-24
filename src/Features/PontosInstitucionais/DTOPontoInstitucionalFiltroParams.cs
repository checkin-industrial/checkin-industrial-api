namespace AppTurismoIndustrial.Api.Features.PontosInstitucionais;

public class DTOPontoInstitucionalFiltroParams
{
    public string? Tipo { get; set; }

    // "true"/"false"/"todos"/null. Parsing via FilterHelpers.ParseAtivo.
    // String tri-state (em vez de bool?) converge com o padrao do painel
    // (dropdown "Todas/Ativos/Inativos" pode passar "todos" explicito).
    public string? Ativo { get; set; }
}
