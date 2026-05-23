# Feature: Analytics

Endpoints analiticos derivados das outras features (sem entidade propria).
Atualmente: heatmap industrial (concentracao de empresas por coordenada).

## Endpoints (sob `/api/analytics`)

| Verbo | Path        | Handler file |
|-------|-------------|--------------|
| GET   | `/heatmap`  | `GetHeatmap.cs` |

Params via `DTOConsultaMapaCalorIndustrial` (`AsParameters`): `Setor`, `Cnae`, raio, etc.

## Services

- `IHeatmapService` / `HeatmapService` — agrega pontos para o heatmap a partir de `Empresas`.
- `IMapaCalorIndustrialService` / `MapaCalorIndustrialService` — wrapper de negocio.
- `IMapaCalorIndustrialQuery` / `MapaCalorIndustrialQuery` — query EF Core direta.
- `HeatmapQueryPoint` — record interno para resultado da query (nao exposto na API).

## Gotchas

- **Setor validation**: `industria`, `comercio`, `servicos` (case-insensitive). Validacao inline no
  endpoint (`GetHeatmap.cs:ValidarConsulta`).
- **CNAE**: 4 a 7 digitos numericos. So digitos sao considerados (qualquer outro caractere e removido
  via regex antes da validacao de tamanho).
- **Cancelamento**: exception `OperationCanceledException` mapeada para `400` com `ProblemDetails`
  (essa logica esta INLINE no endpoint hoje; quando o middleware global cobrir todos os casos,
  remover dali).
- **Exceptions genericas**: capturadas no endpoint e retornam `500` com `ProblemDetails`. Idem ao
  ponto acima — eventualmente unificar com o middleware global.
