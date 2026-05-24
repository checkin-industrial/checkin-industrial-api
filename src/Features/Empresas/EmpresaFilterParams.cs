namespace AppTurismoIndustrial.Api.Features.Empresas;

/// <summary>
/// Representa os parâmetros de filtro recebidos via query string na API.
/// Todos os campos são opcionais para permitir combinações flexíveis.
/// </summary>
public class EmpresaFilterParams
{
    public string? NomeFantasia { get; set; }

    public string? Setor { get; set; }

    public string? Porte { get; set; }

    public string? Cnae { get; set; }

    public string? Municipio { get; set; }

    public string? Situacao { get; set; }

    public int? MinFuncionarios { get; set; }

    public int? MaxFuncionarios { get; set; }

    // "ativo" | "inativo" | "aguardando-revisao" | "todos" | null. Omitir = todos.
    // "ativos" / "inativos" aceitos por compatibilidade com chamadas antigas do painel.
    public string? Status { get; set; }
}
