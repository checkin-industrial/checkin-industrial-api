using AppTurismoIndustrial.Api.Features.Analytics;
using AppTurismoIndustrial.Api.Features.Empresas;
using AppTurismoIndustrial.Api.Features.Empresas.Importacao;
using AppTurismoIndustrial.Api.Features.Geocoding;
using AppTurismoIndustrial.Api.Features.PontosInstitucionais;
using AppTurismoIndustrial.Api.Features.PontosInstitucionais.Importacao;
using AppTurismoIndustrial.Api.Features.TelefonesUteis;
using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using AppTurismoIndustrial.Api.Shared.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Banco de dados ─────────────────────────────────────────────────────────
// AppDbContext aplica IEntityTypeConfiguration de todo o assembly,
// entao basta cada feature definir sua EF mapping no proprio diretorio.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnectionTurismo"),
        npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
    )
);

// ─── Infra compartilhada ────────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddProblemDetails();

// ─── Features (cada module registra os proprios servicos) ───────────────────
builder.Services
    .AddEmpresasFeature()
    .AddImportacaoEmpresasFeature()
    .AddPontosInstitucionaisFeature()
    .AddTelefonesUteisFeature()
    .AddAnalyticsFeature()
    .AddGeocodingFeature();

// ─── CORS (permissivo por enquanto - apertar em PR de seguranca) ────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// ─── Swagger / OpenAPI ──────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ─── Migrations automaticas no startup ──────────────────────────────────────
// TODO: em prod com multiplas instancias, mover para job dedicado de deploy.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ─── Middleware pipeline ────────────────────────────────────────────────────
app.UseProblemDetailsMiddleware();

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("./swagger/v1/swagger.json", "Turismo Empresarial - API");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors();

// Se UPLOADS_ROOT estiver definido (ex.: volume do Railway montado em /uploads),
// serve arquivos estaticos da pasta sob o path /uploads.
var uploadsRoot = app.Configuration["UPLOADS_ROOT"];
if (!string.IsNullOrWhiteSpace(uploadsRoot) && Directory.Exists(uploadsRoot))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsRoot),
        RequestPath = "/uploads",
    });
}
else
{
    app.UseStaticFiles();
}

app.UseAuthorization();

// ─── Endpoints (cada module mapeia seus proprios) ───────────────────────────
app.MapEmpresasEndpoints();
app.MapImportacaoEmpresasEndpoints();
app.MapPontosInstitucionaisEndpoints();
app.MapImportacaoPontosInstitucionaisEndpoints();
app.MapTelefonesUteisEndpoints();
app.MapAnalyticsEndpoints();
app.MapGeocodingEndpoints();

app.Run();
