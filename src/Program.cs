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
// Header X-Api-Key. Em Development, se Auth:ApiKey vazio os requests caem como
// anonimos (util pra rodar local sem config). Em prod, fail-fast: o startup
// aborta se Auth:ApiKey nao for definido (evita endpoints de escrita abertos).
var apiKey = builder.Configuration["Auth:ApiKey"] ?? string.Empty;
if (string.IsNullOrWhiteSpace(apiKey) && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "Auth:ApiKey nao esta configurado. Em prod, defina Auth__ApiKey via env (ex: 'openssl rand -hex 32'). " +
        "Em dev, defina ASPNETCORE_ENVIRONMENT=Development para rodar sem auth.");
}
builder.Services
    .AddAuthentication(ApiKeyAuthenticationOptions.Scheme)
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        ApiKeyAuthenticationOptions.Scheme,
        options => options.ApiKey = apiKey);
builder.Services.AddAuthorization();

// ─── Health checks ──────────────────────────────────────────────────────────
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<AppDbContext>(name: "postgres", tags: new[] { "ready" });

// ─── CORS ───────────────────────────────────────────────────────────────────
// Whitelist via config (Cors:AllowedOrigins). Em Development sem origens configuradas,
// permite qualquer origem com warning. Em prod, fail-fast: o startup aborta se nao
// houver whitelist configurada (evita CORS aberto silenciosamente).
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
if (allowedOrigins.Length == 0 && !builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "Cors:AllowedOrigins nao esta configurado. Em prod, defina Cors__AllowedOrigins__0, Cors__AllowedOrigins__1, ... " +
        "para evitar CORS aberto. Em dev, defina ASPNETCORE_ENVIRONMENT=Development.");
}
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
            // Caminho so atingivel em Development; em prod o throw acima ja abortou.
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

// Em Development com CORS/Auth nao configurados, logar so de aviso (em prod o startup ja abortou).
if (allowedOrigins.Length == 0)
{
    app.Logger.LogWarning("CORS aberto (AllowAnyOrigin) - so e seguro em Development.");
}
if (string.IsNullOrWhiteSpace(apiKey))
{
    app.Logger.LogWarning("Auth:ApiKey nao configurado - endpoints de escrita estao desprotegidos.");
}

// ─── Migrations automaticas no startup ──────────────────────────────────────
// TODO: em prod com multiplas instancias, mover para job dedicado de deploy.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// ─── Pipeline ───────────────────────────────────────────────────────────────
// ForwardedHeaders: por padrao o ASP.NET Core so confia em loopback como proxy,
// entao em Railway/Docker behind any other IP o X-Forwarded-For seria ignorado.
// Como nao temos como saber o IP do proxy do PaaS de antemao e o app sempre
// roda atras de um proxy gerenciado, limpamos KnownNetworks/KnownProxies para
// aceitar qualquer proxy upstream. (Aceitavel porque a infra controla quem
// pode falar HTTP direto com o app - no Railway, so o LB chega ate aqui.)
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownIPNetworks = { },
    KnownProxies = { },
});

app.UseProblemDetailsMiddleware();
app.UseResponseCompression();

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

// Auth precisa rodar ANTES do rate limiter para que o particionador
// veja httpContext.User.Identity.IsAuthenticated e aplique o limite correto.
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
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
