using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTurismoIndustrial.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTiposPontosInstitucionais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE pontos_institucionais
                SET "Tipo" = CASE
                    WHEN "Tipo" = 1 THEN 5
                    WHEN "Tipo" = 2 THEN 1
                    WHEN "Tipo" = 3 THEN 5
                    WHEN "Tipo" = 99 THEN 4
                    ELSE "Tipo"
                END;
                """);

            migrationBuilder.Sql(
                """
                UPDATE pontos_institucionais
                SET "Tipo" = 1
                WHERE lower("Nome") LIKE '%senai%';
                """);

            migrationBuilder.Sql(
                """
                UPDATE pontos_institucionais
                     SET "Tipo" = 5
                     WHERE lower("Nome") LIKE '%prefeitura%'
                         OR lower("Nome") LIKE '%sedecom%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE pontos_institucionais
                SET "Tipo" = CASE
                    WHEN "Tipo" = 5 THEN 1
                    WHEN "Tipo" = 1 THEN 2
                    WHEN "Tipo" = 4 THEN 99
                    ELSE "Tipo"
                END;
                """);
        }
    }
}
