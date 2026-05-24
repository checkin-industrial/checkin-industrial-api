namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

// Mapeamento entre o "tipo" pedido pelo admin (alto-nivel, dominio do produto) e
// os types do Google Places v1 (https://developers.google.com/maps/documentation/places/web-service/place-types).
//
// O setor da Empresa e decisao local (Industria/Comercio/Servicos). A escolha aqui
// foi Opcao 1 do plano (reusar Setor existente) para evitar migration extra. Cada
// tipo de busca mapeia tambem o Setor default a usar pra empresas cadastradas.
public static class GooglePlaceTypeMapping
{
    public record TipoBusca(
        string Slug,
        IReadOnlyList<string> GooglePlaceTypes,
        SetorEmpresa SetorDefault);

    public static readonly IReadOnlyList<TipoBusca> All = new List<TipoBusca>
    {
        new("industria", new[] { "industrial_park" }, SetorEmpresa.Industria),
        new("loja", new[] { "store" }, SetorEmpresa.Comercio),
        new("supermercado", new[] { "supermarket" }, SetorEmpresa.Comercio),
        new("farmacia", new[] { "pharmacy" }, SetorEmpresa.Comercio),
        new("restaurante", new[] { "restaurant" }, SetorEmpresa.Servicos),
        new("hotel", new[] { "hotel", "lodging" }, SetorEmpresa.Servicos),
        new("posto-combustivel", new[] { "gas_station" }, SetorEmpresa.Comercio),
        new("banco", new[] { "bank", "atm" }, SetorEmpresa.Servicos),
        new("oficina-mecanica", new[] { "car_repair" }, SetorEmpresa.Servicos),
        new("loja-veiculos", new[] { "car_dealer" }, SetorEmpresa.Comercio),
    };

    public static TipoBusca? FindBySlug(string slug) =>
        All.FirstOrDefault(t => t.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));

    public static IEnumerable<string> SupportedSlugs => All.Select(t => t.Slug);
}
