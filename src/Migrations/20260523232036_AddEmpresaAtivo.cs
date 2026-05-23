using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTurismoIndustrial.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddEmpresaAtivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // defaultValue: true garante que linhas pre-existentes (criadas antes do
            // campo Ativo existir) entrem como ativas. Sem isso, ficariam NULL e o
            // codigo (que faz `Ativo ?? true`) trataria como ativo igualmente, mas
            // queries futuras de "WHERE Ativo = false" nao pegariam linhas legadas.
            // Coluna permanece nullable porque Telefone/Ponto sao nullable -
            // consistencia entre as 3 features.
            migrationBuilder.AddColumn<bool>(
                name: "Ativo",
                table: "empresas",
                type: "boolean",
                nullable: true,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "empresas");
        }
    }
}
