# Feature: Empresas

Cadastro de empresas + filtros + geolocalizacao. Sub-feature: `Importacao/` (CSV/JSON import + CSV export).

## Modelo

`Empresa.cs` — entidade EF Core com:
- `Cnpj` (string 14 digitos, **unique index** `ux_empresas_cnpj` definido em `EmpresaConfiguration.cs`)
- `Setor`, `Porte`, `MatrizOuFilial`, `SituacaoCadastral` — enums no mesmo arquivo
- `Latitude`/`Longitude` — `decimal(9,6)`

## Endpoints (montados em `EmpresasModule.cs` sob `/api/empresas`)

| Verbo  | Path                       | Handler file               |
|--------|----------------------------|----------------------------|
| GET    | `/`                        | `ListEmpresas.cs` (lista para mapa via `IEmpresaMapService`) |
| GET    | `/filter`                  | `FilterEmpresas.cs` (filtros via `EmpresaFilterParams` AsParameters) |
| GET    | `/{id}/neighbors`          | `GetEmpresaNeighbors.cs` (raio + limite) |
| GET    | `/{id}`                    | `GetEmpresaById.cs` |
| POST   | `/`                        | `CreateEmpresa.cs` (retorna 409 em CNPJ duplicado) |
| PUT    | `/{id}`                    | `UpdateEmpresa.cs` (retorna 409 em CNPJ duplicado) |
| DELETE | `/{id}`                    | `DeleteEmpresa.cs` |

Geocoding endpoint historico em `POST /api/empresas/geocode` mora em `Features/Geocoding/GeocodeAddress.cs`.

## Services

- `IEmpresaService` / `EmpresaService` — CRUD + validacoes de unicidade CNPJ.
- `IEmpresaMapService` / `EmpresaMapService` — projecao leve para o mapa (so campos visuais).
- `IEmpresaFilterService` / `EmpresaFilterService` — filtros multidimensionais.
- `IEmpresaNeighborhoodService` / `EmpresaNeighborhoodService` — vizinhanca por raio (Haversine).
- `IEmpresaFilterQuery` / `EmpresaFilterQuery` — query EF Core pura (separada do service por complexidade).

## Gotchas

- **Enums como string no JSON**: Setor/Porte/MatrizOuFilial/SituacaoCadastral/Status sao
  serializados como string camelCase em todos os endpoints REST (`"industria"`, `"aguardandoRevisao"`).
  A deserializacao aceita TANTO string QUANTO int (`allowIntegerValues: true` em
  `JsonStringEnumConverter`), entao clients legados que mandam ints continuam funcionando -
  use sempre as strings em codigo novo. Config em `Program.cs` na secao "JSON: enums como string".
- **Setor enum/string no CSV**: o import CSV usa strings amigaveis
  (`industria`, `comercio`, `servicos`) - a conversao mora em `Importacao/EmpresaCsvFormatter.cs`.
- **CNPJ unique constraint**: violacao retorna 409 (nao 400/500). Cuide ao alterar fluxos de Create/Update
  pra continuar mapeando `cnpjDuplicado: true` -> `TypedResults.Conflict<object>(...)`.
- **Latitude/Longitude**: precisao `decimal(9,6)`. Latitude valida: -90 a 90, Longitude: -180 a 180
  (`[Range]` no entity).
- **Migracoes**: alteracoes em `Empresa.cs` ou `EmpresaConfiguration.cs` exigem `dotnet ef migrations add`.
  Rodar a partir de `src/`.

## Importacao (sub-feature)

Ver `Importacao/CLAUDE.md` (a criar quando relevante). Resumo:

- POST `/api/import/empresas` aceita CSV ou JSON multipart.
- GET `/api/import/empresas/exportar` retorna CSV UTF-8 BOM.
- GET `/api/import/empresas/exportar-ansi` retorna CSV Windows-1252 (Excel legacy).
- Helpers compartilhados em `EmpresaCsvFormatter.cs` (formatacao de CNPJ com `'` prefix, coordenadas pt-BR).

## GoogleMapsImport (sub-feature)

POST `/api/empresas/import/google-maps` (**X-Api-Key**). Recebe `{ cep, raioMetros, tipo }`,
geocodifica o CEP via Nominatim, chama Google Places Nearby Search, e:

- Cria empresas novas com `Status=AguardandoRevisao` + `GooglePlaceId` setado. Admin revisa e promove para `Ativo` (ou `Inativo` se rejeitar).
- **Enriquece** empresas existentes que ja tem o mesmo `GooglePlaceId` (preenche campos vazios
  com dados do Google; nunca sobrescreve).
- Persiste cada operacao em `google_maps_import_log` (jsonb com response raw + contadores).

Config:

- `GoogleMaps__ApiKey` (env var) — **nunca commitar**. Fail-fast se vazio quando o endpoint e chamado.
- `GoogleMaps__MaxRaioMetros` (default 10000) — cap absoluto, requests acima retornam 400.
- `GoogleMaps__AllowedRegion__LatMin/LatMax/LngMin/LngMax` — bounding box opcional. Quando configurada,
  protege contra requests acidentais fora da regiao prevista (CEP geocodificado fora da box → 400).

**Cnpj nullable**: imports do Google nao trazem CNPJ. Admin preenche depois via Update antes de reativar.
Unique index e parcial (`WHERE Cnpj IS NOT NULL`), entao multiplos nulls coexistem.

Tipos suportados: `industria`, `loja`, `supermercado`, `farmacia`, `restaurante`, `hotel`,
`posto-combustivel`, `banco`, `oficina-mecanica`, `loja-veiculos`. Ver `GooglePlaceTypeMapping.cs`.

**Testes nao devem tocar a API real** — Google Maps cobra por chamada. Mock `IGooglePlacesClient`
em unit tests; use WireMock no docker-compose dos E2E (suite ainda nao implementada).
