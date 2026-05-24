# checkin-industrial-api

Backend **.NET 10** + **EF Core** + **PostgreSQL** da plataforma [Check-in Industrial](https://github.com/checkin-industrial/checkin-industrial-docs/wiki).

[![CI](https://github.com/checkin-industrial/checkin-industrial-api/actions/workflows/ci.yml/badge.svg)](https://github.com/checkin-industrial/checkin-industrial-api/actions/workflows/ci.yml)
[![Docker Hub](https://img.shields.io/badge/docker-checkinindustrial%2Fcheckin--industrial--api-blue?logo=docker)](https://hub.docker.com/r/checkinindustrial/checkin-industrial-api)

---

📖 **Documentação completa** no [Wiki](https://github.com/checkin-industrial/checkin-industrial-docs/wiki)
📄 **Apresentação comercial**: [PDF](https://github.com/checkin-industrial/checkin-industrial-docs/blob/main/Apresentacao_Comercial_Plataforma_Industrial.pdf)

## O que é

API REST com 5 features expostas em **Minimal APIs**: Empresas, Pontos Institucionais, Telefones Úteis, Analytics e Geocoding. Endpoints públicos (anônimos com output cache) para o widget e endpoints de escrita protegidos por `X-Api-Key` para a área administrativa.

## Stack

- **.NET 10 LTS** + Minimal APIs (sem controllers)
- **EF Core 10** + `Npgsql.EntityFrameworkCore.PostgreSQL`
- **Vertical Slice Architecture** (uma feature = uma pasta auto-contida)
- **Swashbuckle.AspNetCore 10** (Swagger UI na raiz)
- **xUnit + Moq** para testes
- **API Key auth** + Rate Limiting + Output Caching + Response Compression + Health Checks

Sem MediatR, sem FluentValidation, sem AutoMapper, sem JWT.

## Estrutura

```text
src/
├── CLAUDE.md                  (tech stack + estrutura + scripts)
├── Program.cs                 (~220 linhas; DI, middleware, MapEndpoints)
├── Migrations/                (EF Core migrations centralizadas)
├── Infrastructure/Persistence/AppDbContext.cs
├── Shared/                    (Errors, Auth, Middleware)
├── Features/                  (VSA — ver Features/CLAUDE.md)
│   ├── Empresas/              (CRUD + filter + neighbors + import CSV)
│   ├── PontosInstitucionais/  (CRUD + upload + import)
│   ├── TelefonesUteis/        (CRUD simples)
│   ├── Analytics/             (heatmap industrial)
│   └── Geocoding/             (Nominatim wrapper)
└── tests/                     (xUnit, espelhando Features/)
```

Mais detalhes em [`src/CLAUDE.md`](src/CLAUDE.md) e [`src/Features/CLAUDE.md`](src/Features/CLAUDE.md).

## Como rodar

### Via Docker Compose (recomendado)

```bash
cd ../checkin-industrial-tests-e2e/docker-compose
docker compose up --build
```

API em <http://localhost:8080> (Swagger na raiz).

### Standalone (dev iteration)

Requer Postgres local em `localhost:5432`:

```bash
cd src
dotnet run
```

## Scripts

| Comando | Função |
|---|---|
| `dotnet build` | Compila Release |
| `dotnet test` | Roda os 22 testes xUnit |
| `dotnet ef migrations add <Nome>` | Cria nova migration (rode em `src/`) |
| `docker build .` | Builda a imagem Docker localmente |

## Variáveis de ambiente

| Var | Default | Função |
|---|---|---|
| `ConnectionStrings__DefaultConnectionTurismo` | `Server=localhost;...` | Connection string Postgres |
| `Auth__ApiKey` | `""` | API Key para endpoints de escrita. **Em prod, vazio aborta o startup**. |
| `Cors__AllowedOrigins__0`, ... | `localhost:5173`, `localhost:8081` | Whitelist CORS. **Em prod, vazio aborta o startup**. |
| `RateLimit__AnonymousPermitPerMinute` | `60` | Reqs/min para reads anônimos |
| `RateLimit__AuthenticatedPermitPerMinute` | `300` | Reqs/min para writes autenticados |
| `OutputCache__ReadEndpointTtlSeconds` | `60` | TTL do output cache em reads |
| `UPLOADS_ROOT` | `wwwroot/uploads` | Volume montado para uploads de imagem |
| `PORT` | `8080` | Porta HTTP |

Para gerar uma API Key: `openssl rand -hex 32`.

## CI/CD

- **`ci.yml`** — `dotnet restore` + `build` + `test` em cada push e PR
- **`publish-docker.yml`** — em push para `main` e em tags `v*.*.*`, builda multi-arch (amd64+arm64) e publica em [`checkinindustrial/checkin-industrial-api`](https://hub.docker.com/r/checkinindustrial/checkin-industrial-api) no Docker Hub

Detalhes em [Wiki — CI/CD](https://github.com/checkin-industrial/checkin-industrial-docs/wiki/Para-Devs-CI-CD).

## Convenções

A arquitetura segue **VSA** — cada feature é auto-contida em `src/Features/<X>/`. Convenções obrigatórias (anti-patterns que evitar, padrão de naming, como adicionar feature/endpoint novo) estão em [`src/Features/CLAUDE.md`](src/Features/CLAUDE.md).

Antes de abrir PR: `dotnet build` + `dotnet test` precisam estar verdes.

## Repositórios irmãos

| Repo | Papel |
|---|---|
| [`checkin-industrial-painel`](https://github.com/checkin-industrial/checkin-industrial-painel) | Frontend React |
| [`checkin-industrial-tests-e2e`](https://github.com/checkin-industrial/checkin-industrial-tests-e2e) | Suíte E2E Robot Framework |
| [`checkin-industrial-docs`](https://github.com/checkin-industrial/checkin-industrial-docs) | Wiki + apresentação |
