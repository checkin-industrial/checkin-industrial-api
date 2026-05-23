using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTurismoIndustrial.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTelefonesUteisLayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "telefones_uteis",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Categoria = table.Column<int>(type: "integer", nullable: false),
                    Telefone = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    OrdemExibicao = table.Column<int>(type: "integer", nullable: true),
                    Ativo = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_telefones_uteis", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "telefones_uteis",
                columns: new[] { "Id", "Nome", "Categoria", "Telefone", "OrdemExibicao", "Ativo" },
                values: new object[,]
                {
                    { new Guid("b1000000-0000-0000-0000-000000000001"), "Conselho Tutelar", 1, "(14) 3264-7987", 1, true },
                    { new Guid("b1000000-0000-0000-0000-000000000002"), "Corpo de Bombeiros", 1, "193", 2, true },
                    { new Guid("b1000000-0000-0000-0000-000000000003"), "Delegacia", 1, "(14) 3263-0101", 3, true },
                    { new Guid("b1000000-0000-0000-0000-000000000004"), "Guarda Civil Municipal", 1, "153", 4, true },
                    { new Guid("b1000000-0000-0000-0000-000000000005"), "Hospital N. Sra. Piedade", 1, "(14) 3269-1033", 5, true },
                    { new Guid("b1000000-0000-0000-0000-000000000006"), "Paco Municipal", 1, "(14) 3269-7000", 6, true },
                    { new Guid("b1000000-0000-0000-0000-000000000007"), "Resgate Integrado", 1, "0800 700 192", 7, true },
                    { new Guid("b1000000-0000-0000-0000-000000000008"), "Policia Militar", 1, "190", 8, true },
                    { new Guid("b1000000-0000-0000-0000-000000000009"), "SAMU", 1, "192", 9, true },
                    { new Guid("b1000000-0000-0000-0000-000000000010"), "UPA", 1, "(14) 3269-1300", 10, true },
                    { new Guid("b2000000-0000-0000-0000-000000000001"), "Ponto de Taxi (Centro)", 2, "(14) 3263-0099", 1, true },
                    { new Guid("b2000000-0000-0000-0000-000000000002"), "Ponto de Taxi (Terminal Rodoviario)", 2, "(14) 3436-1616", 2, true },
                    { new Guid("b2000000-0000-0000-0000-000000000003"), "Secretaria de Turismo", 2, "(14) 3263-0445", 3, true },
                    { new Guid("b2000000-0000-0000-0000-000000000004"), "Secretaria de Cultura", 2, "(14) 3263-6525", 4, true },
                    { new Guid("b3000000-0000-0000-0000-000000000001"), "7 Hotel", 3, "(14) 99123-8538", 1, true },
                    { new Guid("b3000000-0000-0000-0000-000000000002"), "Casagrande Hotel", 3, "(14) 3263-0749", 2, true },
                    { new Guid("b3000000-0000-0000-0000-000000000003"), "Novotel Lencois", 3, "(14) 3436-2310", 3, true },
                    { new Guid("b3000000-0000-0000-0000-000000000004"), "Hotel Colonial Suites", 3, "(14) 99805-9045 / 3436-1344", 4, true },
                    { new Guid("b3000000-0000-0000-0000-000000000005"), "Hospedaria Inamagil", 3, "(14) 99605-0529 / 99854-2026", 5, true },
                    { new Guid("b3000000-0000-0000-0000-000000000006"), "Hotel Pousada do Leao", 3, "(14) 3264-6363 / 99695-9216", 6, true },
                    { new Guid("b3000000-0000-0000-0000-000000000007"), "Izi Hotel", 3, "(14) 3278-2999", 7, true },
                    { new Guid("b3000000-0000-0000-0000-000000000008"), "Passer Hotel", 3, "(14) 3269-1700", 8, true },
                    { new Guid("b3000000-0000-0000-0000-000000000009"), "Pousada Dona Dutra", 3, "(14) 98176-9112 / 99700-6364", 9, true },
                    { new Guid("b3000000-0000-0000-0000-000000000010"), "Pensao Sao Jose", 3, "(14) 3263-1172", 10, true },
                    { new Guid("b3000000-0000-0000-0000-000000000011"), "Pousada Sao Joao", 3, "(14) 3264-6649 / 99844-7779", 11, true },
                    { new Guid("b3000000-0000-0000-0000-000000000012"), "Saint James Palace Hotel", 3, "(14) 3263-6758", 12, true },
                    { new Guid("b3000000-0000-0000-0000-000000000013"), "Villas Plaza Hotel", 3, "(14) 99113-5362 / 3269-3990", 13, true },
                    { new Guid("b3000000-0000-0000-0000-000000000014"), "VR2 Hotel", 3, "(14) 3436-1900", 14, true },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "telefones_uteis");
        }
    }
}
