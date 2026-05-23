
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppTurismoIndustrial.Api.Features.Empresas;

public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
{
    public void Configure(EntityTypeBuilder<Empresa> builder)
    {
        builder.ToTable("empresas");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Cnpj)
            .IsRequired()
            .HasMaxLength(14);

        builder.HasIndex(e => e.Cnpj)
            .IsUnique()
            .HasDatabaseName("ux_empresas_cnpj");

        builder.Property(e => e.RazaoSocial)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.NomeFantasia)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.CnaePrincipal)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.Endereco)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Telefone)
            .IsRequired(false)
            .HasMaxLength(20);

        builder.Property(e => e.Cep)
            .IsRequired(false)
            .HasMaxLength(8);

        builder.Property(e => e.NumeroFuncionarios)
            .IsRequired(false);

        builder.Property(e => e.Municipio)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.DescricaoCnae)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(e => e.Latitude)
            .HasPrecision(9, 6);

        builder.Property(e => e.Longitude)
            .HasPrecision(9, 6);

        builder.Property(e => e.DataCadastro)
            .IsRequired();
    }
}
