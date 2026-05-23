# Feature: PontosInstitucionais

Cadastro de pontos institucionais (educacao, hoteis, turismo, etc.) com upload de imagens.

## Modelo

`PontoInstitucional.cs` — entidade EF Core. Sem `IEntityTypeConfiguration<>` proprio
(so Data Annotations). Campos chave:
- `Tipo`: `TipoPontoInstitucional` (Educacao, Comercio, Financeiro, Servico, SetorPrefeitura, PontoTuristico, Hotel, Ecoturismo)
- `Latitude`/`Longitude`: `decimal(9,6)` (Range validado)
- URLs de imagens: `LogoUrl`, `CardFotoUrl`, `ResponsavelFotoUrl` (relativas, ex: `/uploads/pontos-institucionais/logo/xxx.png`)

## Endpoints (sob `/api/pontos-institucionais`)

| Verbo  | Path             | Handler file |
|--------|------------------|--------------|
| GET    | `/`              | `ListPontosInstitucionais.cs` |
| GET    | `/{id}`          | `GetPontoInstitucionalById.cs` |
| POST   | `/`              | `CreatePontoInstitucional.cs` |
| PUT    | `/{id}`          | `UpdatePontoInstitucional.cs` |
| DELETE | `/{id}`          | `DeletePontoInstitucional.cs` |
| POST   | `/upload-imagem` | `UploadImagemPontoInstitucional.cs` (multipart, max 5 MB) |

## Gotchas

- **Upload de imagem**: extensoes permitidas: `.jpg .jpeg .png .webp .svg`. MIME types validados separadamente.
  Categoria do upload via form field `categoria` (`logo` | `card` | qualquer outro -> `foto`).
- **Path de upload**: se a env `UPLOADS_ROOT` estiver definida (volume montado em prod), arquivos vao para
  `{UPLOADS_ROOT}/pontos-institucionais/<categoria>/`. Senao, `wwwroot/uploads/pontos-institucionais/<categoria>/`.
- **DisableAntiforgery**: o endpoint de upload precisa `.DisableAntiforgery()` em Minimal APIs, senao o
  framework recusa multipart sem token antiforgery.
- **TipoPontoInstitucional**: 8 valores. Parser em `Importacao/PontoInstitucionalCsvFormatter.cs` aceita
  strings amigaveis (`setor_prefeitura`, `ponto_turistico`) alem dos numericos.

## Importacao (sub-feature)

Em `Importacao/`. Endpoints sob `/api/import/pontos-institucionais...`.
A logica de parse CSV mora **inline no endpoint** `ImportPontosInstitucionais.cs` (nao ha service
intermediario como em Empresas). Os helpers de parse (decimal pt-BR/EN, bool tolerante, tipo enum)
estao em `PontoInstitucionalCsvFormatter.cs`.
