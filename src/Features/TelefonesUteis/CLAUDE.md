# Feature: TelefonesUteis

Cadastro CRUD de telefones uteis (emergencia, hoteis, transporte, etc.). Feature simples
sem sub-features, sem import/export, sem upload — bom template pra novas features.

## Modelo

`TelefoneUtil.cs`:
- `Categoria`: `CategoriaTelefoneUtil` (EmergenciaServicosPublicos, TransporteCultura, HoteisPousadas)
- `OrdemExibicao` opcional (ordenacao no painel)
- `Ativo` opcional (soft hide)

Sem `IEntityTypeConfiguration<>` proprio.

## Endpoints (sob `/api/telefones-uteis`)

| Verbo  | Path     | Handler file |
|--------|----------|--------------|
| GET    | `/`      | `ListTelefonesUteis.cs` (filtros via `DTOTelefoneUtilFiltroParams` AsParameters) |
| GET    | `/{id}`  | `GetTelefoneUtilById.cs` |
| POST   | `/`      | `CreateTelefoneUtil.cs` |
| PUT    | `/{id}`  | `UpdateTelefoneUtil.cs` |
| DELETE | `/{id}`  | `DeleteTelefoneUtil.cs` |

## Services

- `ITelefoneUtilService` / `TelefoneUtilService` — CRUD basico.
- `ITelefoneUtilQuery` / `TelefoneUtilQuery` — query EF Core para filtros.

## Gotchas

- Sem unique constraints. Multiplos telefones com mesmo numero sao validos (pode ter o mesmo numero
  em categorias diferentes).
