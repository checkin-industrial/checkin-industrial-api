

using AppTurismoIndustrial.Api.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Banco de Dados ───────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnectionTurismo"),
        npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)
    )
);

// ─── Cache ────────────────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// ─── Serviços de Aplicação ────────────────────────────────────────────────────
builder.Services.AddScoped<IEmpresaService, EmpresaService>();
builder.Services.AddScoped<IEmpresaNeighborhoodService, EmpresaNeighborhoodService>();
builder.Services.AddScoped<IEmpresaMapService, EmpresaMapService>();
builder.Services.AddScoped<IEmpresaFilterService, EmpresaFilterService>();
builder.Services.AddScoped<IEmpresaFilterQuery, EmpresaFilterQuery>();
builder.Services.AddScoped<IPontoInstitucionalService, PontoInstitucionalService>();
builder.Services.AddScoped<IPontoInstitucionalQuery, PontoInstitucionalQuery>();
builder.Services.AddScoped<ITelefoneUtilService, TelefoneUtilService>();
builder.Services.AddScoped<ITelefoneUtilQuery, TelefoneUtilQuery>();
builder.Services.AddScoped<IMapaCalorIndustrialQuery, MapaCalorIndustrialQuery>();
builder.Services.AddScoped<IHeatmapService, HeatmapService>();
builder.Services.AddScoped<IGeocodingProvider, StubGeocodingProvider>();
builder.Services.AddScoped<IGeocodingService, GeocodingService>();
builder.Services.AddScoped<IImportacaoEmpresasService, ImportacaoEmpresasService>();

// ─── Controllers ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ─── Swagger ─────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ─── Migrations automáticas ao iniciar ───────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    //app.UseSwaggerUI();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("./swagger/v1/swagger.json", "Turismo Empresarial - API");
        options.RoutePrefix = string.Empty;        
    });
}

app.UseHttpsRedirection();
app.UseCors();

// Se UPLOADS_ROOT estiver definido (ex.: volume do Railway montado em /uploads),
// serve os arquivos estáticos dessa pasta sob o path /uploads.
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
app.MapControllers();

app.Run();
