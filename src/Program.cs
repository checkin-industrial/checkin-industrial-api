using System.Threading.RateLimiting;
using AppTurismoIndustrial.Api.Features.Analytics;
using AppTurismoIndustrial.Api.Features.Empresas;
using AppTurismoIndustrial.Api.Features.Empresas.Importacao;
using AppTurismoIndustrial.Api.Features.Geocoding;
using AppTurismoIndustrial.Api.Features.PontosInstitucionais;
using AppTurismoIndustrial.Api.Features.PontosInstitucionais.Importacao;
using AppTurismoIndustrial.Api.Features.TelefonesUteis;
using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using AppTurismoIndustrial.Api.Shared.Auth;
using AppTurismoIndustrial.Api.Shared.Middleware;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Banco de dados ─────────────────────────────────────────────────────────
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
builder.Services.AddOutputCache(options =>
{
    var ttl = builder.Configuration.GetValue<int?>("OutputCache:ReadEndpointTtlSeconds") ?? 60;
    options.AddPolicy("ReadEndpoint", b => b.Expire(TimeSpan.FromSeconds(ttl)));
});

// ─── Response compression ───────────────────────────────────────────────────
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
    {
        "application/json",
        "application/problem+json",
        "text/csv"
    });
});

// ─── Rate limiting ──────────────────────────────────────────────────────────
// Fixed window por IP. Limites configuraveis. Anonimos sao mais restritos que autenticados.
var anonPerMin = builder.Configuration.GetValue<int?>("RateLimit:AnonymousPermitPerMinute") ?? 60;
var authPerMin = builder.Configuration.GetValue<int?>("RateLimit:AuthenticatedPermitPerMinute") ?? 300;
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var isAuthenticated = httpContext.User?.Identity?.IsAuthenticated ?? false;
        var key = httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        var limit = isAuthenticated ? authPerMin : anonPerMin;
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = limit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
            AutoReplenishment = true,
        });
    });
});

// ─── Autenticacao via API Key ───────────────────────────────────────────────
// Header X-Api-Key. Se Auth:ApiKey nao estiver configurado, todos os requests
// caem como anonimos (util pra dev). Em prod, definir via env Auth__ApiKey.
builder.Services
    .AddAuthentication(ApiKeyAuthenticationOptions.Scheme)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationOptions.Scheme,
        options => options.ApiKey = builder.Configuration["Auth:ApiKey"] ?? string.Empty);
builder.Services.AddAuthorization();

// ─── Health checks ──────────────────────────────────────────────────────────
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "postgres", tags: new[] { "ready" });

// ─── CORS ───────────────────────────────────────────────────────────────────
// Whitelist via config (Cors:AllowedOrigins). Em ambiente sem origens configuradas
// (ex.: smoke test local), permite qualquer origem com warning no log.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
        else
        {
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    });
});

// ─── Features ───────────────────────────────────────────────────────────────
builder.Services
    .AddEmpresasFeature()
    .AddImportacaoEmpresasFeature()
    .AddPontosInstitucionaisFeature()
    .AddTelefonesUteisFeature()
    .AddAnalyticsFeature()
    .AddGeocodingFeature();

// ─── Swagger / OpenAPI ──────────────────────────────────────────────────────
// TODO: adicionar SecurityDefinition para X-Api-Key na UI do Swagger.
// Swashbuckle 10 mudou a API do Microsoft.OpenApi.Models - olhar em proximo PR.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Aviso se CORS esta aberto em prod por config faltante
if (allowedOrigins.Length == 0 && !app.Environment.IsDevelopment())
{
    app.Logger.LogWarning("CORS aberto (AllowAnyOrigin) - configure Cors:AllowedOrigins via env Cors__AllowedOrigins__0/1/...");
}
if (string.IsNullOrWhiteSpace(builder.Configuration["Auth:ApiKey"]) && !app.Environment.IsDevelopment())
{
    app.Logger.LogWarning("Auth:ApiKey nao configurado - endpoints de escrita estao desprotegidos. Configure via env Auth__ApiKey.");
}

// ─── Migrations automaticas no startup ──────────────────────────────────────
// TODO: em prod com multiplas instancias, mover para job dedicado de deploy.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ─── Pipeline ───────────────────────────────────────────────────────────────
// Forwarded headers (Railway/proxy reverso devolve real IP em X-Forwarded-For)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseProblemDetailsMiddleware();
app.UseResponseCompression();
app.UseRateLimiter();

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

// Se UPLOADS_ROOT estiver definido, serve arquivos estaticos do volume.
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

app.UseAuthentication();
app.UseAuthorization();
app.UseOutputCache();

// ─── Endpoints ──────────────────────────────────────────────────────────────
app.MapHealthChecks("/health");

app.MapEmpresasEndpoints();
app.MapImportacaoEmpresasEndpoints();
app.MapPontosInstitucionaisEndpoints();
app.MapImportacaoPontosInstitucionaisEndpoints();
app.MapTelefonesUteisEndpoints();
app.MapAnalyticsEndpoints();
app.MapGeocodingEndpoints();

app.Run();
