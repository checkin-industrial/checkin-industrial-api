using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTurismoIndustrial.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTurismoHotelEcoturismo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Pontos Turísticos e Serviços (Tipo = 6)
            migrationBuilder.InsertData(
                table: "pontos_institucionais",
                columns: new[]
                {
                    "Id", "Nome", "Tipo", "Descricao", "Endereco", "Latitude", "Longitude",
                    "AtividadesDisponiveis", "EquipeGestao", "ContatoNome", "ContatoTelefone", "ContatoEmail",
                    "ResponsavelFotoUrl", "LogoUrl", "CorMarcador", "IconeMarcador", "OrdemExibicao", "Ativo"
                },
                values: new object[,]
                {
                    {
                        new Guid("a1000000-0000-0000-0000-000000000001"),
                        "SETUR / Posto de Informações Turísticas (PIT)",
                        6,
                        "Posto de informacoes turisticas da Secretaria de Turismo de Lencois Paulista.",
                        "Avenida 25 de Janeiro, 699 - Centro - Lençóis Paulista",
                        -22.596174m, -48.799404m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 100, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000002"),
                        "Lanchódromo Municipal / Ponto de Táxi",
                        6,
                        "Espaco gastronômico municipal com opcoes de alimentacao e ponto de taxi.",
                        "Rua Dr. Antônio Tedesco, 70 - Centro - Lençóis Paulista",
                        -22.596098m, -48.799149m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 101, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000003"),
                        "Estação Ferroviária / Terminal Interbairros",
                        6,
                        "Complexo com estacao ferroviaria historica e terminal de onibus interbairros.",
                        "Pátio da Estação, 1 (próximo à Rodoviária) - Lençóis Paulista",
                        -22.595125m, -48.797637m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 102, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000004"),
                        "Biblioteca Municipal Orígenes Lessa",
                        6,
                        "Biblioteca publica municipal Origenes Lessa, com acervo literario e cultural.",
                        "Praça Comendador José Zillo, Centro - Lençóis Paulista",
                        -22.598043m, -48.800383m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 103, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000005"),
                        "Espaço Cultural Cidade do Livro",
                        6,
                        "Espaco cultural dedicado a literatura e atividades educativas e artisticas.",
                        "Rua Pedro Natálio Lorenzetti, 286 - Centro - Lençóis Paulista",
                        -22.595549m, -48.799883m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 104, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000006"),
                        "Casa de Cultura Maria Bove Coneglian",
                        6,
                        "Centro cultural com exposicoes, apresentacoes e promocao da cultura local.",
                        "Rua Sete de Setembro, 934 - Centro - Lençóis Paulista",
                        -22.599039m, -48.798938m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 105, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000007"),
                        "Museu Alexandre Chitto",
                        6,
                        "Museu com acervo historico e cultural da regiao de Lencois Paulista.",
                        "Rua Cel. Joaquim Anselmo Martins, 575 - Centro - Lençóis Paulista",
                        -22.596929m, -48.797080m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 106, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000008"),
                        "Santuário Nossa Senhora da Piedade",
                        6,
                        "Santuario religioso de Nossa Senhora da Piedade, ponto de fe e devocao.",
                        "Rua Sete de Setembro, 1054 - Centro - Lençóis Paulista",
                        -22.599034m, -48.800742m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 107, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000009"),
                        "Igreja Presbiteriana Independente",
                        6,
                        "Igreja historica no centro de Lencois Paulista.",
                        "R. Pedro Natálio Lorenzeti, 511 - Centro, Lençóis Paulista - SP",
                        -22.599826m, -48.801471m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 108, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000010"),
                        "Parque do Paradão / Letreiro Eu Amo LP",
                        6,
                        "Parque com area de lazer e letreiro iconico EU AMO LP.",
                        "Rua 28 de Abril (esquina com a Rua 15 de Novembro) - Centro - Lençóis Paulista",
                        -22.598129m, -48.806098m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 109, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000011"),
                        "Rio Lençóis",
                        6,
                        "Rio que corta o municipio, com areas de lazer e beleza natural.",
                        "Lençóis Paulista - SP",
                        -22.528515m, -48.912656m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 110, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000012"),
                        "Praça das Palmeiras / Paço Municipal",
                        6,
                        "Praca simbolica da cidade, sede do Paco Municipal.",
                        "Praça das Palmeiras, 55 - Centro - Lençóis Paulista",
                        -22.598912m, -48.792313m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 111, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000013"),
                        "Teatro Municipal Adélia Lorenzetti",
                        6,
                        "Teatro municipal Adelia Lorenzetti, palco de espetaculos e eventos culturais.",
                        "Rua Cel. Álvaro Martins, 790 - Vila Nova Irerê - Lençóis Paulista",
                        -22.609035m, -48.801106m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 112, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000014"),
                        "Orquidário Municipal Welthes Repik",
                        6,
                        "Orquidario municipal com colecao de orquideas e plantas ornamentais.",
                        "Rua Cel. Álvaro Martins, 820 (ao lado do Teatro) - Lençóis Paulista",
                        -22.609464m, -48.801028m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 113, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000015"),
                        "Jardim Sensorial e Mini Zoológico de Esculturas",
                        6,
                        "Jardim sensorial com esculturas de animais, espaco de lazer e inclusao.",
                        "R. Cel. Álvaro Martins, 790 - Centro, Lençóis Paulista - SP, 18682-180",
                        -22.609035m, -48.801106m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 114, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000016"),
                        "Recinto da Facilpa",
                        6,
                        "Recinto da Facilpa, local de feiras, exposicoes e eventos agropecuarios e industriais.",
                        "Avenida Lázaro Brígido Dutra, 300 - Lençóis Paulista",
                        -22.612440m, -48.801415m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 115, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000017"),
                        "Praça da Juventude",
                        6,
                        "Espaco de lazer e convivencia para jovens com quadras e areas verdes.",
                        "Parque Antartica, Lençóis Paulista - SP, 18683-600",
                        -22.605964m, -48.804696m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 116, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000018"),
                        "Lago da Prata",
                        6,
                        "Lago artificial para lazer com areas de caminhada e contemplacao.",
                        "Av. Lázaro Brígido Dutra - Jardim Lago da Prata, Lençóis Paulista - SP",
                        -22.613062m, -48.800612m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 117, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000019"),
                        "Parque do Povo / Letreiro EU Amo LENÇÓIS",
                        6,
                        "Parque publico com letreiro EU AMO LENCOIS, area de lazer e eventos ao ar livre.",
                        "Avenida Lázaro Brígido Dutra (adjacente ao Recinto da Facilpa) - Lençóis Paulista",
                        -22.612691m, -48.801284m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 118, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000020"),
                        "Terminal Rodoviário",
                        6,
                        "Terminal rodoviario municipal para transporte intermunicipal e regional.",
                        "Avenida Padre Vicente de Paulo Penido, 165 - Lençóis Paulista",
                        -22.598912m, -48.792313m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 119, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000021"),
                        "Distrito Empresarial",
                        6,
                        "Distrito empresarial com concentracao de industrias e comercios.",
                        "Rua Ásia, Lençóis Paulista",
                        -22.562706m, -48.819769m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 120, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000022"),
                        "Vinícola Casagrande",
                        6,
                        "Vinicola local com producao artesanal de vinhos e ponto de visitacao.",
                        "Rua Ernesto - Lençóis Paulista",
                        -22.586579m, -48.796915m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 121, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000023"),
                        "Engenho São Luiz",
                        6,
                        "Engenho historico para visitacao, producao de cachaca e turismo rural.",
                        "Rodovia Osny Matheus (SP-261), km 111 - Lençóis Paulista",
                        -22.601721m, -48.813945m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 122, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000024"),
                        "Cemitério Municipal Alcides Francisco",
                        6,
                        "Cemiterio municipal com area historica e patrimônio cultural local.",
                        "Estrada Municipal LEP-454, km 0 - Lençóis Paulista",
                        -22.539074m, -48.821857m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 123, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000025"),
                        "Cemitério Parque Paraíso da Colina",
                        6,
                        "Cemiterio parque particular para sepultamento e visitacao.",
                        "R. Geraldo Pereira de Barros, 255 - Centro, Lençóis Paulista - SP",
                        -22.599863m, -48.793210m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 124, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000026"),
                        "Cemitério Parque Irmãos Panico",
                        6,
                        "Cemiterio parque com espaco para sepultamento e homenagens.",
                        "Av. Jacomo Augusto Paccola, 1400 - Lençóis Paulista, SP",
                        -22.613902m, -48.787575m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 125, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000027"),
                        "Estação Ferroviária Alfredo Guedes",
                        6,
                        "Estacao ferroviaria historica do distrito de Alfredo Guedes.",
                        "Av. Jacomo Augusto Paccola, 2740 - Nucleo Hab. Joao Zillo, Lençóis Paulista - SP",
                        -22.603513m, -48.781121m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 126, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000028"),
                        "Capela do Senhor Bom Jesus",
                        6,
                        "Capelinha historica do Senhor Bom Jesus no distrito de Alfredo Guedes.",
                        "R. Sete de Setembro, 90 - Alfredo Guedes, Lençóis Paulista - SP",
                        -22.594165m, -48.711623m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 127, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000029"),
                        "Capela de São Benedito",
                        6,
                        "Capelinha de Sao Benedito em area rural de Lencois Paulista.",
                        "Acesso pela Rodovia Marechal Rondon (SP-300) - Lençóis Paulista",
                        -22.748461m, -48.584881m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 128, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000030"),
                        "Memorial de Alfredo Guedes",
                        6,
                        "Memorial dedicado ao fundador do distrito de Alfredo Guedes.",
                        "Avenida 25 de Janeiro, s/n - Centro - Lençóis Paulista",
                        -22.596710m, -48.797209m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 129, true
                    },
                    {
                        new Guid("a1000000-0000-0000-0000-000000000031"),
                        "Letreiro SOU+ALFREDO GUEDES",
                        6,
                        "Letreiro simbolico do distrito de Alfredo Guedes, ponto fotografico.",
                        "R. João Gasparini - Alfredo Guedes, Lençóis Paulista - SP",
                        -22.593178m, -48.712900m,
                        "", "", "", "", "", null, null, "#ea580c", "turistico", 130, true
                    },
                });

            // Hospedagem / Hotéis (Tipo = 7)
            migrationBuilder.InsertData(
                table: "pontos_institucionais",
                columns: new[]
                {
                    "Id", "Nome", "Tipo", "Descricao", "Endereco", "Latitude", "Longitude",
                    "AtividadesDisponiveis", "EquipeGestao", "ContatoNome", "ContatoTelefone", "ContatoEmail",
                    "ResponsavelFotoUrl", "LogoUrl", "CorMarcador", "IconeMarcador", "OrdemExibicao", "Ativo"
                },
                values: new object[,]
                {
                    {
                        new Guid("a2000000-0000-0000-0000-000000000001"),
                        "Z Hotel",
                        7,
                        "Hotel em Alfredo Guedes, opcao de hospedagem no distrito.",
                        "Alfredo Guedes, Lençóis Paulista - SP",
                        -22.589677m, -48.713396m,
                        "", "", "", "", "", null, null, "#7c3aed", "hotel", 200, true
                    },
                    {
                        new Guid("a2000000-0000-0000-0000-000000000002"),
                        "Novotel Lençóis",
                        7,
                        "Hotel Novotel, uma das melhores opcoes de hospedagem no centro de Lencois Paulista.",
                        "Rua Dr. Antônio Tedesco, 16 - Centro - Lençóis Paulista",
                        -22.595249m, -48.799150m,
                        "", "", "", "", "", null, null, "#7c3aed", "hotel", 201, true
                    },
                    {
                        new Guid("a2000000-0000-0000-0000-000000000003"),
                        "Hotel Colonial Suites",
                        7,
                        "Hotel com ambiente colonial e conforto para hospedes.",
                        "Rua Ignácio Anselmo Martins, 700 - Lençóis Paulista",
                        -22.603048m, -48.800601m,
                        "", "", "", "", "", null, null, "#7c3aed", "hotel", 202, true
                    },
                    {
                        new Guid("a2000000-0000-0000-0000-000000000004"),
                        "Hospedaria Inamagil",
                        7,
                        "Hospedaria familiar com boa localizacao em Lencois Paulista.",
                        "Avenida Lázaro Brígido Dutra, 881 - Lençóis Paulista",
                        -22.623714m, -48.792805m,
                        "", "", "", "", "", null, null, "#7c3aed", "hotel", 203, true
                    },
                    {
                        new Guid("a2000000-0000-0000-0000-000000000005"),
                        "Hotel Pousada do Leão",
                        7,
                        "Pousada aconchegante no centro historico da cidade.",
                        "R. Dona Januária, 10 - Centro, Lençóis Paulista - SP, 18680-047",
                        -22.595326m, -48.798573m,
                        "", "", "", "", "", null, null, "#7c3aed", "hotel", 204, true
                    },
                    {
                        new Guid("a2000000-0000-0000-0000-000000000006"),
                        "Izi Hotel",
                        7,
                        "Hotel moderno com conforto e boa localizacao em Lencois Paulista.",
                        "Av. Papa João Paulo II, 374 - Vila Antonieta II, Lençóis Paulista - SP",
                        -22.597350m, -48.787563m,
                        "", "", "", "", "", null, null, "#7c3aed", "hotel", 205, true
                    },
                    {
                        new Guid("a2000000-0000-0000-0000-000000000007"),
                        "Passer Hotel",
                        7,
                        "Hotel com estrutura completa para hospedes corporativos e turistas.",
                        "R. Adriano Anderson Foganholi, 1550 - Nucleo Hab. Luis Zillo, Lençóis Paulista - SP",
                        -22.576018m, -48.811440m,
                        "", "", "", "", "", null, null, "#7c3aed", "hotel", 206, true
                    },
                    {
                        new Guid("a2000000-0000-0000-0000-000000000008"),
                        "Pousada Dona Dutra",
                        7,
                        "Pousada familiar com ambiente tranquilo e acolhedor.",
                        "Rua Manoel Amâncio, 100 - Lençóis Paulista",
                        -22.599620m, -48.794484m,
                        "", "", "", "", "", null, null, "#7c3aed", "hotel", 207, true
                    },
                    {
                        new Guid("a2000000-0000-0000-0000-000000000009"),
                        "Pensão São José",
                        7,
                        "Opcao economica de hospedagem em Lencois Paulista.",
                        "R. Armando Aguinaga, 35 - Parque Antartica, Lençóis Paulista - SP",
                        -22.604878m, -48.805526m,
                        "", "", "", "", "", null, null, "#7c3aed", "hotel", 208, true
                    },
                    {
                        new Guid("a2000000-0000-0000-0000-000000000010"),
                        "Saint James Palace Hotel",
                        7,
                        "Hotel de categoria com estrutura completa para hospedes.",
                        "R. Silvio Bosi, 123 - Centro, Lençóis Paulista - SP",
                        -22.611970m, -48.792385m,
                        "", "", "", "", "", null, null, "#7c3aed", "hotel", 209, true
                    },
                    {
                        new Guid("a2000000-0000-0000-0000-000000000011"),
                        "Villas Plaza Hotel",
                        7,
                        "Hotel bem localizado no centro de Lencois Paulista.",
                        "R. Pedro Natálio Lorenzeti, 115 - Centro, Lençóis Paulista - SP",
                        -22.596018m, -48.799905m,
                        "", "", "", "", "", null, null, "#7c3aed", "hotel", 210, true
                    },
                    {
                        new Guid("a2000000-0000-0000-0000-000000000012"),
                        "VR2 Hotel",
                        7,
                        "Hotel com estrutura moderna e conforto para hospedes.",
                        "Av. Padre Salustio Rodrigues Machado, 1380 - Jardim Ubirama, Lençóis Paulista - SP",
                        -22.602315m, -48.810833m,
                        "", "", "", "", "", null, null, "#7c3aed", "hotel", 211, true
                    },
                });

            // Ecoturismo (Tipo = 8)
            migrationBuilder.InsertData(
                table: "pontos_institucionais",
                columns: new[]
                {
                    "Id", "Nome", "Tipo", "Descricao", "Endereco", "Latitude", "Longitude",
                    "AtividadesDisponiveis", "EquipeGestao", "ContatoNome", "ContatoTelefone", "ContatoEmail",
                    "ResponsavelFotoUrl", "LogoUrl", "CorMarcador", "IconeMarcador", "OrdemExibicao", "Ativo"
                },
                values: new object[,]
                {
                    {
                        new Guid("a3000000-0000-0000-0000-000000000001"),
                        "Trilhas do Arvrão (acesso Via Bichos do Mato)",
                        8,
                        "Acesso as trilhas do Arvrao pelo ponto Bichos do Mato, ideal para caminhadas e contato com a natureza.",
                        "Av. Lázaro Brígido Dutra, 700 - Centro, Lençóis Paulista - SP",
                        -22.615293m, -48.799450m,
                        "", "", "", "", "", null, null, "#16a34a", "ecoturismo", 300, true
                    },
                    {
                        new Guid("a3000000-0000-0000-0000-000000000002"),
                        "Trilhas do Arvrão (acesso Via do Sol)",
                        8,
                        "Acesso as trilhas do Arvrao pela Via do Sol, com percursos em meio a vegetacao nativa.",
                        "Lençóis Paulista - SP",
                        -22.621023m, -48.811898m,
                        "", "", "", "", "", null, null, "#16a34a", "ecoturismo", 301, true
                    },
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove Ecoturismo (Tipo = 8)
            migrationBuilder.DeleteData(table: "pontos_institucionais", keyColumn: "Id", keyValue: new Guid("a3000000-0000-0000-0000-000000000001"));
            migrationBuilder.DeleteData(table: "pontos_institucionais", keyColumn: "Id", keyValue: new Guid("a3000000-0000-0000-0000-000000000002"));

            // Remove Hotéis (Tipo = 7)
            migrationBuilder.DeleteData(table: "pontos_institucionais", keyColumn: "Id", keyValue: new Guid("a2000000-0000-0000-0000-000000000001"));
            migrationBuilder.DeleteData(table: "pontos_institucionais", keyColumn: "Id", keyValue: new Guid("a2000000-0000-0000-0000-000000000002"));
            migrationBuilder.DeleteData(table: "pontos_institucionais", keyColumn: "Id", keyValue: new Guid("a2000000-0000-0000-0000-000000000003"));
            migrationBuilder.DeleteData(table: "pontos_institucionais", keyColumn: "Id", keyValue: new Guid("a2000000-0000-0000-0000-000000000004"));
            migrationBuilder.DeleteData(table: "pontos_institucionais", keyColumn: "Id", keyValue: new Guid("a2000000-0000-0000-0000-000000000005"));
            migrationBuilder.DeleteData(table: "pontos_institucionais", keyColumn: "Id", keyValue: new Guid("a2000000-0000-0000-0000-000000000006"));
            migrationBuilder.DeleteData(table: "pontos_institucionais", keyColumn: "Id", keyValue: new Guid("a2000000-0000-0000-0000-000000000007"));
            migrationBuilder.DeleteData(table: "pontos_institucionais", keyColumn: "Id", keyValue: new Guid("a2000000-0000-0000-0000-000000000008"));
            migrationBuilder.DeleteData(table: "pontos_institucionais", keyColumn: "Id", keyValue: new Guid("a2000000-0000-0000-0000-000000000009"));
            migrationBuilder.DeleteData(table: "pontos_institucionais", keyColumn: "Id", keyValue: new Guid("a2000000-0000-0000-0000-000000000010"));
            migrationBuilder.DeleteData(table: "pontos_institucionais", keyColumn: "Id", keyValue: new Guid("a2000000-0000-0000-0000-000000000011"));
            migrationBuilder.DeleteData(table: "pontos_institucionais", keyColumn: "Id", keyValue: new Guid("a2000000-0000-0000-0000-000000000012"));

            // Remove Pontos Turísticos (Tipo = 6)
            for (int i = 1; i <= 31; i++)
            {
                migrationBuilder.DeleteData(
                    table: "pontos_institucionais",
                    keyColumn: "Id",
                    keyValue: new Guid($"a1000000-0000-0000-0000-{i:D12}"));
            }
        }
    }
}
