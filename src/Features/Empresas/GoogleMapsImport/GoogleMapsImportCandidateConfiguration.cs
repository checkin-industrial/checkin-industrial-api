using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTurismoIndustrial.Api.Features.Empresas.GoogleMapsImport;

public class GoogleMapsImportCandidateConfiguration : IEntityTypeConfiguration<GoogleMapsImportCandidate>
{
    public void Configure(EntityTypeBuilder<GoogleMapsImportCandidate> builder)
    {
        builder.ToTable("google_maps_import_candidates");

        builder.HasKey(c => c.Id);

        // Unique index: dedup ao re-importar o mesmo place — atualiza dados em vez
        // de criar candidato duplicado. Diferente do Empresa.GooglePlaceId que e
        // unique parcial (admite null), aqui o campo e obrigatorio (todo candidato
        // vem do Google) entao indice pode ser unique simples.
        builder.HasIndex(c => c.GooglePlaceId)
            .IsUnique()
            .HasDatabaseName("ux_import_candidates_place_id");

        // Indice por log pra listar candidatos de um import especifico no painel.
        builder.HasIndex(c => c.GoogleMapsImportLogId)
            .HasDatabaseName("ix_import_candidates_log");

        // Indices auxiliares pros filtros mais comuns na tela de triagem
        // (mostrar pendentes por destino).
        builder.HasIndex(c => c.EmpresaStatus)
            .HasDatabaseName("ix_import_candidates_empresa_status");
        builder.HasIndex(c => c.PontoStatus)
            .HasDatabaseName("ix_import_candidates_ponto_status");
        builder.HasIndex(c => c.TelefoneStatus)
            .HasDatabaseName("ix_import_candidates_telefone_status");

        builder.HasOne(c => c.Log)
            .WithMany()
            .HasForeignKey(c => c.GoogleMapsImportLogId)
            .OnDelete(DeleteBehavior.Cascade);

        // Precisao decimal alinhada com Empresa.Latitude/Longitude (9,6 = ate ~10cm).
        builder.Property(c => c.Latitude).HasPrecision(9, 6);
        builder.Property(c => c.Longitude).HasPrecision(9, 6);
    }
}
