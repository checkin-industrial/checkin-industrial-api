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

- **Setor enum/string**: o painel manda `Setor` como int via query (`?setor=1`), mas o CSV usa strings
  (`industria`, `comercio`, `servicos`). A conversao mora em `Importacao/EmpresaCsvFormatter.cs`.
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
