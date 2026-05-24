namespace AppTurismoIndustrial.Api.Shared.Filters;

// Helpers transversais pra parsing de query params de filter DTOs.
// Cada feature mantem seu DTO proprio (sem heranca - VSA) mas usa esses
// helpers no service/query para padronizar o comportamento de filtros
// que aparecem em multiplas features (atualmente apenas `ativo`).
//
// Criterio para algo viver aqui (vs em <Feature>Query/Service):
//   - Aparece em >= 2 features, OU
//   - E claramente transversal (paginacao, ordering, filtro de visibilidade)
//
// Ver Shared/Filters/CLAUDE.md.
public static class FilterHelpers
{
    /// <summary>
    /// Parseia o valor de um query param "ativo" textual para bool? tri-state.
    /// Aceita: "true"/"True"/"TRUE" -> true, "false" -> false, "todos"/null/""/"qualquer-coisa" -> null.
    ///
    /// Padrao para features cuja visibilidade tem 2 estados (Ativo/Inativo) - i.e.,
    /// PontoInstitucional e TelefoneUtil. Empresa tem 3 estados (StatusEmpresa enum)
    /// e parsing proprio em EmpresaFilterQuery.ParseStatus.
    /// </summary>
    public static bool? ParseAtivo(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "true" or "ativo" or "ativos" => true,
            "false" or "inativo" or "inativos" => false,
            // "todos" e qualquer string nao reconhecida caem em null (sem filtro).
            _ => null,
        };
    }
}
