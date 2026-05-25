# Feature: Geocoding

Servico cross-feature de geocodificacao (endereco -> lat/lon) via OpenStreetMap Nominatim,
com cache em memoria de 30 dias.

## Endpoint historico

| Verbo | Path                    | Handler file |
|-------|-------------------------|--------------|
| POST  | `/api/empresas/geocode` | `GeocodeAddress.cs` |

⚠️ A rota mora sob `/api/empresas/` por motivos historicos (era um endpoint do `EmpresasController`).
O painel atual depende dela. Antes de renomear pra `/api/geocode`, alinhar com `checkin-industrial-painel`.

## Servicos

- `IGeocodingService` / `GeocodingService` — fachada com cache. Quando o input casa
  `ViaCepClient.CepRegex` (ex: "18681420", "CEP 18681-420"), invoca o ViaCEP primeiro
  pra resolver CEP -> endereco estruturado (logradouro, bairro, localidade, UF) e
  reusa essa string completa como query do Nominatim. Sem ViaCEP, Nominatim falha
  em muitos CEPs do interior — testes mostraram CEP de Lencois Paulista sendo
  geolocalizado em Araraquara (50 km de erro).
- `IGeocodingProvider` (interface) — abstracao do provedor.
- `StubGeocodingProvider` — implementacao via Nominatim OSM. Apesar do nome "stub", e funcional;
  a nomenclatura veio de quando era stubbed. Pode ser renomeado pra `NominatimGeocodingProvider`
  num PR futuro (atualizar registro DI em `GeocodingModule.cs`).
- `IViaCepClient` / `ViaCepClient` — resolve CEP brasileiro via [ViaCEP](https://viacep.com.br)
  (publico, gratuito, sem auth). Returna `ViaCepAddress` ou `null` em CEP invalido /
  erro de rede (o GeocodingService faz fallback pra Nominatim direto nesse caso).
- `GeocodeResult` (model interno) — definido em `IGeocodingService.cs` (campos `Latitude`, `Longitude`,
  `Accuracy`, `Provider`, `ObtainedAt`).

## Cache

- Storage: `IMemoryCache` (registrado em `Program.cs`).
- TTL: 30 dias absoluto, 7 dias sliding.
- Resultados negativos (nao encontrados) sao cacheados por 7 dias para evitar retry de enderecos invalidos.
- `ClearCache()` e `GetCacheSize()` no service sao stubs — `IMemoryCache` nao expoe API de listing.
  Pra controle mais granular, migrar pra Redis ou implementar `IMemoryCache` customizado.

## Gotchas

- **User-Agent obrigatorio**: Nominatim exige User-Agent identificavel. Setado em
  `StubGeocodingProvider.cs` como `AppTurismoIndustrial/1.0`.
- **Rate limit OSM**: ~1 req/s pelo termo de uso publico. Hoje nao ha throttling; em volume alto,
  considerar provedor pago (Google Maps) ou self-hosted Nominatim.
- **Endereco brasileiro**: queries sempre concatenam `, Brasil` no final pra reduzir falsos positivos.
