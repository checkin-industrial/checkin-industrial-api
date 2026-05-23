# checkin-industrial-api

Backend .NET 9 + EF Core + PostgreSQL para a plataforma checkin-industrial.

## Stack

- **.NET 9** + Minimal APIs (sem `[ApiController]` + controllers)
- **EF Core 9** + `Npgsql.EntityFrameworkCore.PostgreSQL` (provider)
- **CsvHelper** para importacao/exportacao de cadastros
- **Swashbuckle.AspNetCore** (Swagger UI servida em `/` na raiz)
- **xUnit + Moq** para testes
- Sem MediatR, sem FluentValidation, sem AutoMapper

## Estrutura do diretorio `src/`

```
src/
├── CLAUDE.md                       (este arquivo)
├── Program.cs                      (~80 linhas; DI por feature, middleware, MapEndpoints)
├── AppTurismoIndustrial.Api.csproj (global usings para todas as features)
├── AppTurismoIndustrial.Api.sln
├── appsettings.json                (defaults locais: Server=localhost, Password=postgres)
├── appsettings.Development.json
├── Properties/
├── Migrations/                     (EF Core migrations, gerenciadas centralmente)
├── Infrastructure/
│   └── Persistence/
│       └── AppDbContext.cs         (unico; DbSet de cada entidade)
├── Shared/                         (codigo cross-feature - regra dos 2-3 usos)
│   ├── Errors/AppException.cs      (NotFoundException, ValidationException, ConflictException)
│   └── Middleware/ProblemDetailsMiddleware.cs (RFC 7807, captura AppException + Exception)
├── Features/                       (ver Features/CLAUDE.md para convencoes)
│   ├── CLAUDE.md
│   ├── Empresas/                   (CRUD + filter + heatmap-neighbors + import/export)
│   ├── PontosInstitucionais/       (CRUD + upload + import/export)
│   ├── TelefonesUteis/             (CRUD simples)
│   ├── Analytics/                  (heatmap industrial)
│   └── Geocoding/                  (geocoding via Nominatim/OSM)
└── tests/
    └── AppTurismoIndustrial.Api.Tests/
        ├── AppTurismoIndustrial.Api.Tests.csproj (ProjectReference + global usings)
        └── Services/               (xUnit, espelhando a estrutura de Features/)
```

## Arquitetura: Vertical Slice (VSA) + convencoes de IA

Em vez de Clean Arch (Domain / Application / Infrastructure / Api separados), agrupamos
todo o codigo de uma feature em uma so pasta. Isso reduz o numero de arquivos que um
agente de IA (ou humano) precisa abrir para entender ou modificar a feature.

**Princípios:**

1. **Uma feature = uma pasta auto-contida** sob `Features/<Feature>/`.
2. **Um endpoint = um arquivo**, nomeado pelo verbo + entidade: `CreateEmpresa.cs`,
   `ListPontosInstitucionais.cs`, `ImportEmpresas.cs`.
3. **Cada feature tem um `<Feature>Module.cs`** que registra DI + mapeia os endpoints.
4. **Minimal APIs** com `TypedResults<>` para tipagem forte dos status HTTP.
5. **Erros via excecoes + middleware ProblemDetails** (RFC 7807). Lance `NotFoundException`,
   `ValidationException`, `ConflictException` em vez de retornar `IActionResult.NotFound()`
   nos services. (Hoje os endpoints ainda checam booleanos retornados pelo service; novo
   codigo deve preferir excecoes.)
6. **Sem `using AppTurismoIndustrial.Api.Features.*`** nos arquivos: os namespaces ja
   estao listados como **global usings** em `.csproj`. Quem consome uma feature de
   outra ve os tipos diretamente.
7. **Validacao**: Data Annotations no DTO de request (ex.: `[Required]`, `[Range]`,
   `[StringLength]`). Para regras que nao cabem em atributos, valide inline no handler
   do endpoint e retorne `BadRequest`.

## Como adicionar um endpoint novo

1. Crie `Features/<Feature>/<Verbo><Entidade>.cs` (ex.: `Features/Empresas/ArchiveEmpresa.cs`).
2. Pattern do arquivo:
   ```csharp
   namespace AppTurismoIndustrial.Api.Features.Empresas;

   public static class ArchiveEmpresa
   {
       public static RouteGroupBuilder MapArchiveEmpresa(this RouteGroupBuilder group)
       {
           group.MapPost("/{id:guid}/archive", Handle).WithName(nameof(ArchiveEmpresa));
           return group;
       }

       private static async Task<Results<NoContent, NotFound>> Handle(
           Guid id, IEmpresaService service, CancellationToken ct)
       {
           var ok = await service.ArquivarAsync(id, ct);
           return ok ? TypedResults.NoContent() : TypedResults.NotFound();
       }
   }
   ```
3. Adicione `group.MapArchiveEmpresa();` no `<Feature>Module.cs`.

## Como rodar localmente

Pré-requisitos: .NET 9 SDK + Docker (para Postgres).

```bash
# Postgres + API + Painel via docker-compose (recomendado):
cd ../../checkin-industrial-tests-e2e/docker-compose
docker compose up --build

# OU API standalone (precisa de um Postgres local na 5432):
cd src
dotnet run
```

API: <http://localhost:8080> (swagger em `/`). Swagger JSON: `/swagger/v1/swagger.json`.

## Tests

```bash
cd src
dotnet test
```

Testes ficam em `src/tests/` (espelhando `Features/` por feature/service).

## Pontos de atencao / TODOs

- **Migrations no startup** (`Program.cs:50`): `db.Database.Migrate()` roda automatico ao iniciar.
  Em prod com multiplas instancias, mover para job dedicado de deploy.
- **CORS permissivo** (`Program.cs:31`): hoje aceita qualquer origem. Apertar em PR de seguranca.
- **Test coverage do import de pontos**: o test `ImportarPontosInstitucionais_Deve_Normalizar_Coordenadas`
  foi removido (era quebrado, Moq nao consegue construir AppDbContext sem options). Re-adicionar via
  WebApplicationFactory ou testando `PontoInstitucionalCsvFormatter` direto.
- **Geocoding endpoint** em `POST /api/empresas/geocode`: rota historica, logica mora em
  `Features/Geocoding/GeocodeAddress.cs`. Se algum dia mover a rota para `/api/geocode`, alinhar
  com o painel antes.
