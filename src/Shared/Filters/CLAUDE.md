# Shared/Filters

Helpers transversais para tratamento de query params em filter DTOs.

## O que mora aqui

Funcoes puras que aparecem em **>= 2 features** ou sao claramente **transversais**
(paginacao, ordering, filtro de visibilidade).

Atualmente:

- `FilterHelpers.ParseAtivo(string?): bool?` - parseia textual "ativo/inativo/true/false/todos" para tri-state.

## O que NAO mora aqui

Helpers domain-specific de uma feature. Exemplos:

- `ParseSetor`, `ParsePorte`, `ParseSituacao` (Empresa) - moram em `EmpresaFilterQuery.cs`.
- `ParseStatus` (Empresa, enum 3-estados) - mora em `EmpresaFilterQuery.cs` por ter semantica propria (Ativo/Inativo/AguardandoRevisao).
- `ParseCategoria` (TelefoneUtil) - dominio especifico.

## Convencao para DTOs novos

Filter DTOs devem:

1. Manter shape proprio por feature (sem heranca - VSA enfatiza isolamento).
2. Usar `FilterHelpers.ParseAtivo` no service/query se houver filtro de visibilidade `Ativo`.
3. Para filtros dominio-especificos, manter `ParseXxx` privado no proprio `<Feature>Query`.

Quando adicionar helper aqui, atualizar este CLAUDE.md com a regra ("aparece em N features X, Y, Z").

## Naming convention

Para DTOs novos, prefira `<Feature>FilterParams` (em vez de `DTO<Feature>FiltroParams`).
DTOs existentes (`DTOPontoInstitucionalFiltroParams`, `DTOTelefoneUtilFiltroParams`)
ficam como estao - rename e breaking change interno sem ganho funcional.
