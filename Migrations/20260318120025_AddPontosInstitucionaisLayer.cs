using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTurismoIndustrial.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPontosInstitucionaisLayer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pontos_institucionais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Descricao = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Endereco = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Latitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "numeric(9,6)", precision: 9, scale: 6, nullable: false),
                    AtividadesDisponiveis = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    EquipeGestao = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ContatoNome = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    ContatoTelefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ContatoEmail = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ResponsavelFotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CorMarcador = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IconeMarcador = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    OrdemExibicao = table.Column<int>(type: "integer", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pontos_institucionais", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "pontos_institucionais",
                columns: new[]
                {
                    "Id", "Nome", "Tipo", "Descricao", "Endereco", "Latitude", "Longitude",
                    "AtividadesDisponiveis", "EquipeGestao", "ContatoNome", "ContatoTelefone", "ContatoEmail",
                    "ResponsavelFotoUrl", "CorMarcador", "IconeMarcador", "OrdemExibicao", "Ativo"
                },
                values: new object[,]
                {
                    {
                        new Guid("5f774c6a-4bd8-41d8-8f93-4a11991ce8e1"),
                        "SEDECOM Lençóis Paulista",
                        1,
                        "Espaco de treinamento e articulacao com o setor produtivo do municipio.",
                        "Rua Coronel Joaquim Anselmo Martins, Centro, Lencois Paulista - SP",
                        -22.598970m,
                        -48.800220m,
                        "Capacitacao empresarial, orientacao para formalizacao, apoio para editais e rodadas de negocios.",
                        "Equipe de Desenvolvimento Economico",
                        "Coordenacao SEDECOM",
                        "14998000001",
                        "sedecom@lencoispaulista.sp.gov.br",
                        null,
                        "#b91c1c",
                        "sedecom",
                        1,
                        true
                    },
                    {
                        new Guid("f20e17ef-d331-43f7-9838-b36a65f2500e"),
                        "SENAI Lençóis Paulista",
                        2,
                        "Unidade parceira para qualificacao tecnica e inovacao industrial.",
                        "Rua Ignacio Anselmo, Jardim Ubirama, Lencois Paulista - SP",
                        -22.596540m,
                        -48.794970m,
                        "Cursos tecnicos, consultoria de produtividade e trilhas de qualificacao para industria 4.0.",
                        "Equipe de Relacoes com Industria",
                        "Atendimento Corporativo SENAI",
                        "14998000002",
                        "parcerias.senai@sp.senai.br",
                        null,
                        "#1d4ed8",
                        "senai",
                        2,
                        true
                    },
                    {
                        new Guid("f7005f33-e6dc-42a4-a7f0-b13e7e5453f9"),
                        "Setor de Meio Ambiente - Prefeitura",
                        3,
                        "Ponto de apoio para licenciamento, orientacoes ambientais e regularizacao.",
                        "Praca das Palmeiras, Centro, Lencois Paulista - SP",
                        -22.599920m,
                        -48.804840m,
                        "Orientacao ambiental, licencas e apoio para adequacao regulatoria industrial.",
                        "Equipe Tecnica de Meio Ambiente",
                        "Secretaria de Meio Ambiente",
                        "14998000003",
                        "meioambiente@lencoispaulista.sp.gov.br",
                        null,
                        "#0f766e",
                        "prefeitura",
                        3,
                        true
                    },
                    {
                        new Guid("8b5842f4-22f3-4a26-a42e-662f4f9afd1e"),
                        "Setor de Obras e Infraestrutura - Prefeitura",
                        3,
                        "Canal para demandas de infraestrutura urbana e suporte para expansao industrial.",
                        "Avenida Padre Salustio Rodrigues Machado, Centro, Lencois Paulista - SP",
                        -22.601820m,
                        -48.801380m,
                        "Analise de viabilidade, orientacao para obras e acompanhamento de solicitacoes.",
                        "Equipe de Planejamento Urbano",
                        "Secretaria de Obras",
                        "14998000004",
                        "obras@lencoispaulista.sp.gov.br",
                        null,
                        "#0f766e",
                        "prefeitura",
                        4,
                        true
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "pontos_institucionais",
                keyColumn: "Id",
                keyValue: new Guid("5f774c6a-4bd8-41d8-8f93-4a11991ce8e1"));

            migrationBuilder.DeleteData(
                table: "pontos_institucionais",
                keyColumn: "Id",
                keyValue: new Guid("f20e17ef-d331-43f7-9838-b36a65f2500e"));

            migrationBuilder.DeleteData(
                table: "pontos_institucionais",
                keyColumn: "Id",
                keyValue: new Guid("f7005f33-e6dc-42a4-a7f0-b13e7e5453f9"));

            migrationBuilder.DeleteData(
                table: "pontos_institucionais",
                keyColumn: "Id",
                keyValue: new Guid("8b5842f4-22f3-4a26-a42e-662f4f9afd1e"));

            migrationBuilder.DropTable(
                name: "pontos_institucionais");
        }
    }
}
