using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTurismoIndustrial.Api.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceAtivoWithStatusEnum : Migration
    {
        // Empresa.Ativo: bool? -> Empresa.Status: StatusEmpresa (int).
        // Backfill preserva o estado existente das linhas e promove imports do Google
        // (Ativo=false AND GooglePlaceId IS NOT NULL) para AguardandoRevisao em vez de Inativo.
        //
        // Mapeamento:
        //   Ativo IS NULL OR Ativo = true                              -> Status = 1 (Ativo)
        //   Ativo = false AND GooglePlaceId IS NULL                    -> Status = 2 (Inativo)
        //   Ativo = false AND GooglePlaceId IS NOT NULL                -> Status = 3 (AguardandoRevisao)
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1) Adiciona Status com default temporario (Ativo) para evitar NOT NULL violation
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "empresas",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // 2) Backfill a partir do estado existente
            migrationBuilder.Sql(@"
                UPDATE empresas
                SET ""Status"" = CASE
                    WHEN ""Ativo"" IS NULL OR ""Ativo"" = true THEN 1
                    WHEN ""Ativo"" = false AND ""GooglePlaceId"" IS NOT NULL THEN 3
                    ELSE 2
                END;
            ");

            // 3) Remove a coluna antiga
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "empresas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Inverso: recria Ativo nullable + backfill (perde a distincao
            // Inativo vs AguardandoRevisao - ambos viram Ativo=false).
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "empresas",
                type: "boolean",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE empresas
                SET ""Ativo"" = CASE
                    WHEN ""Status"" = 1 THEN true
                    ELSE false
                END;
            ");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "empresas");
        }
    }
}
