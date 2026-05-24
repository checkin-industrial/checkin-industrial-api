using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTurismoIndustrial.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleMapsImport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_empresas_cnpj",
                table: "empresas");

            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                table: "empresas",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14);

            migrationBuilder.AddColumn<string>(
                name: "GooglePlaceId",
                table: "empresas",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "google_maps_import_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Cep = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    RaioMetros = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LatitudeOrigem = table.Column<decimal>(type: "numeric", nullable: false),
                    LongitudeOrigem = table.Column<decimal>(type: "numeric", nullable: false),
                    ResponseRaw = table.Column<string>(type: "jsonb", nullable: false),
                    EmpresasCriadas = table.Column<int>(type: "integer", nullable: false),
                    EmpresasAtualizadas = table.Column<int>(type: "integer", nullable: false),
                    EmpresasIgnoradas = table.Column<int>(type: "integer", nullable: false),
                    Erro = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_google_maps_import_log", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ux_empresas_cnpj",
                table: "empresas",
                column: "Cnpj",
                unique: true,
                filter: "\"Cnpj\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ux_empresas_google_place_id",
                table: "empresas",
                column: "GooglePlaceId",
                unique: true,
                filter: "\"GooglePlaceId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "google_maps_import_log");

            migrationBuilder.DropIndex(
                name: "ux_empresas_cnpj",
                table: "empresas");

            migrationBuilder.DropIndex(
                name: "ux_empresas_google_place_id",
                table: "empresas");

            migrationBuilder.DropColumn(
                name: "GooglePlaceId",
                table: "empresas");

            migrationBuilder.AlterColumn<string>(
                name: "Cnpj",
                table: "empresas",
                type: "character varying(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(14)",
                oldMaxLength: 14,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "ux_empresas_cnpj",
                table: "empresas",
                column: "Cnpj",
                unique: true);
        }
    }
}
