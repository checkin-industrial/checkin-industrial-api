# Features

Cada subpasta aqui e uma feature auto-contida — Vertical Slice Architecture (VSA).

## Anatomia de uma feature

```
Features/<Feature>/
├── CLAUDE.md                       # contratos, gotchas, exemplos
├── <Entidade>.cs                   # POCO EF Core + enums (uma entidade = um arquivo)
├── <Entidade>Configuration.cs      # IEntityTypeConfiguration<T> (so se houver mapeamento alem de Data Annotations)
├── <Feature>Module.cs              # Add<Feature>Feature() + Map<Feature>Endpoints()
├── I<Feature>Service.cs + <Feature>Service.cs   (1+ services, conforme a feature)
├── I<Feature>Query.cs + <Feature>Query.cs       (queries EF Core diretas, opcional)
├── DTO<Algo>.cs                    # requests / responses (records ou classes)
├── List<Entidades>.cs              # endpoints (verbo + entidade): um por arquivo
├── Get<Entidade>By<X>.cs
├── Create<Entidade>.cs
├── Update<Entidade>.cs
├── Delete<Entidade>.cs
└── <SubFeature>/                   # opcional - sub-features substantivamente distintas
    ├── ...                         # ex: Importacao/ tem suas proprias DTOs + endpoints
    └── <SubFeature>Module.cs
```

## Convencoes mandatorias

### 1. Namespace = pasta

`Features/Empresas/Empresa.cs` -> `namespace AppTurismoIndustrial.Api.Features.Empresas;`

Sub-features tem namespace estendido:
`Features/Empresas/Importacao/ImportEmpresas.cs` -> `namespace AppTurismoIndustrial.Api.Features.Empresas.Importacao;`

### 2. Global usings cobrem todas as features

`AppTurismoIndustrial.Api.csproj` lista cada namespace de feature como `<Using Include="..." />`.
**Nao adicione `using AppTurismoIndustrial.Api.Features.*;` em arquivos novos** — eh ruido visual.

Se voce criar uma feature ou sub-feature nova, atualize os global usings em **dois lugares**:
- `src/AppTurismoIndustrial.Api.csproj`
- `src/tests/AppTurismoIndustrial.Api.Tests/AppTurismoIndustrial.Api.Tests.csproj`

### 3. Um endpoint = um arquivo

Cada arquivo de endpoint expoe um `public static class` com:
- Metodo `Map<NomeDoEndpoint>(this RouteGroupBuilder group)` (ou `IEndpointRouteBuilder` quando nao usar group)
- Metodo `Handle(...)` private static (a logica em si)

Pattern:

```csharp
namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public static class CreateTelefoneUtil
{
    public static RouteGroupBuilder MapCreateTelefoneUtil(this RouteGroupBuilder group)
    {
        group.MapPost("/", Handle).WithName(nameof(CreateTelefoneUtil));
        return group;
    }

    private static async Task<Created<DTOTelefoneUtil>> Handle(
        DTOTelefoneUtilCriar dto,
        ITelefoneUtilService service,
        CancellationToken cancellationToken)
    {
        var created = await service.CriarAsync(dto, cancellationToken);
        return TypedResults.Created($"/api/telefones-uteis/{created.Id}", created);
    }
}
```

### 4. `<Feature>Module.cs` orquestra DI e endpoints

Pattern:

```csharp
namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public static class TelefonesUteisModule
{
    public static IServiceCollection AddTelefonesUteisFeature(this IServiceCollection services)
    {
        services.AddScoped<ITelefoneUtilService, TelefoneUtilService>();
        services.AddScoped<ITelefoneUtilQuery, TelefoneUtilQuery>();
        return services;
    }

    public static IEndpointRouteBuilder MapTelefonesUteisEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/telefones-uteis").WithTags("TelefonesUteis");
        group.MapListTelefonesUteis();
        group.MapGetTelefoneUtilById();
        group.MapCreateTelefoneUtil();
        group.MapUpdateTelefoneUtil();
        group.MapDeleteTelefoneUtil();
        return endpoints;
    }
}
```

### 5. Use `TypedResults<>` ao inves de `IActionResult`

`TypedResults.Ok(...)`, `TypedResults.NotFound()`, `TypedResults.Conflict<object>(...)`.

Para handlers que retornam multiplos status codes, use `Results<Ok<T>, NotFound, Conflict<object>>`
no tipo de retorno. Isso documenta os possiveis status no OpenAPI/Swagger automaticamente.

### 6. Validacao

- Data Annotations no DTO de request (`[Required]`, `[Range]`, `[StringLength]`, `[RegularExpression]`).
- Regras nao-anotaveis: valide inline no handler e retorne `BadRequest`.
- Em logica de service, prefira lancar `ValidationException` / `NotFoundException` / `ConflictException`
  do `Shared/Errors/AppException.cs`. O middleware ProblemDetails captura.

### 7. Erros

- Excecoes -> middleware `ProblemDetailsMiddleware` (registrado em `Program.cs`) -> resposta RFC 7807.
- Codigo legacy ainda usa booleanos retornados pelos services. Esta tudo bem; novo codigo prefere excecoes.

## Sub-features (ex.: Importacao)

Quando uma feature tem um grupo coeso de funcionalidades suficientemente grande para ter sua propria identidade (ex.: importacao/exportacao CSV com helpers proprios), use uma subpasta:

```
Empresas/
├── (CRUD basico)
├── EmpresasModule.cs
└── Importacao/
    ├── ImportacaoEmpresasModule.cs  # sub-module
    ├── EmpresaCsvFormatter.cs        # helpers locais a sub-feature
    ├── ImportEmpresas.cs
    ├── ExportEmpresasCsv.cs
    └── ExportEmpresasCsvAnsi.cs
```

Tanto `EmpresasModule.cs` quanto `ImportacaoEmpresasModule.cs` sao chamados separadamente em
`Program.cs` (`app.MapEmpresasEndpoints(); app.MapImportacaoEmpresasEndpoints();`).

## Anti-patterns que evitar

- ❌ Criar `Services/`, `Models/`, `Dtos/` subpastas dentro de uma feature. Mantenha flat.
- ❌ Compartilhar DTOs entre features. Se duas features precisam do mesmo formato, **duplique** o DTO
  (verbosidade > acoplamento). Excecao: cross-feature genuino (ex.: `Pagination<T>`) vai pra `Shared/`.
- ❌ `[ApiController]` + controllers. Migramos para Minimal APIs.
- ❌ Endpoint que aninha multiplos verbos no mesmo arquivo. Um arquivo por verbo.
- ❌ `using AppTurismoIndustrial.Api.Features.X;` no topo de um arquivo. Use os global usings.

## Como criar uma feature do zero

1. `mkdir src/Features/<NovaFeature>`
2. Crie a entidade (se houver) em `<Entidade>.cs` com Data Annotations
3. Crie `<Feature>Module.cs` com `Add<Feature>Feature()` e `Map<Feature>Endpoints()`
4. Crie os endpoint files (`List`, `Get`, `Create`, etc.)
5. Adicione `services.Add<Feature>Feature()` em `Program.cs`
6. Adicione `app.Map<Feature>Endpoints()` em `Program.cs`
7. Adicione o namespace ao `<Using Include="..." />` nos dois `.csproj`
8. Crie `<Feature>/CLAUDE.md` com contratos da feature
9. Adicione `db.<NovaEntidade>` ao `AppDbContext` e gere a migration: `dotnet ef migrations add Add<NovaFeature>`
