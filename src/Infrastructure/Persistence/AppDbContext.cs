
using Microsoft.EntityFrameworkCore;

namespace AppTurismoIndustrial.Api.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<PontoInstitucional> PontosInstitucionais => Set<PontoInstitucional>();
    public DbSet<TelefoneUtil> TelefonesUteis => Set<TelefoneUtil>();
    public DbSet<GoogleMapsImportLog> GoogleMapsImportLogs => Set<GoogleMapsImportLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
