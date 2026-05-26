using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTurismoIndustrial.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddImportCandidates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "google_maps_import_candidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GoogleMapsImportLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    GooglePlaceId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FormattedAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    Telefone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TypesJson = table.Column<string>(type: "jsonb", nullable: false),
                    CepOrigem = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: true),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EmpresaStatus = table.Column<int>(type: "integer", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: true),
                    EmpresaDecididoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PontoStatus = table.Column<int>(type: "integer", nullable: false),
                    PontoInstitucionalId = table.Column<Guid>(type: "uuid", nullable: true),
                    PontoDecididoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TelefoneStatus = table.Column<int>(type: "integer", nullable: false),
                    TelefoneUtilId = table.Column<Guid>(type: "uuid", nullable: true),
                    TelefoneDecididoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_google_maps_import_candidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_google_maps_import_candidates_google_maps_import_log_Google~",
                        column: x => x.GoogleMapsImportLogId,
                        principalTable: "google_maps_import_log",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_import_candidates_empresa_status",
                table: "google_maps_import_candidates",
                column: "EmpresaStatus");

            migrationBuilder.CreateIndex(
                name: "ix_import_candidates_log",
                table: "google_maps_import_candidates",
                column: "GoogleMapsImportLogId");

            migrationBuilder.CreateIndex(
                name: "ix_import_candidates_ponto_status",
                table: "google_maps_import_candidates",
                column: "PontoStatus");

            migrationBuilder.CreateIndex(
                name: "ix_import_candidates_telefone_status",
                table: "google_maps_import_candidates",
                column: "TelefoneStatus");

            migrationBuilder.CreateIndex(
                name: "ux_import_candidates_place_id",
                table: "google_maps_import_candidates",
                column: "GooglePlaceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "google_maps_import_candidates");
        }
    }
}
