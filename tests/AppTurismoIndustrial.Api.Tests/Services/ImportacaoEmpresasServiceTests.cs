using System.Text;
using AppTurismoIndustrial.Api.Application.DTOs.Import;
using AppTurismoIndustrial.Api.Application.Services;
using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AppTurismoIndustrial.Api.Controllers;

namespace AppTurismoIndustrial.Api.Tests.Services;

public class ImportacaoEmpresasServiceTests
{
    [Fact]
    public async Task ImportarAsync_ComCsvValido_DeveInserirRegistros()
    {
        var contextMock = CreateContextMock();
        var loggerMock = new Mock<ILogger<ImportacaoEmpresasService>>();
        var service = new ImportacaoEmpresasService(contextMock.Object, loggerMock.Object);

        var csv = string.Join(Environment.NewLine,
            "CNPJ;Razao_Social;Nome_Fantasia;CNAE_Principal;Setor;Porte;Numero_Funcionarios;Endereco;Telefone;CEP;Municipio;Descricao_CNAE;Matriz_ou_Filial;Latitude;Longitude;Situacao_Cadastral",
            $"{GerarCnpjValido(1)};Empresa Um;Fantasia Um;6201500;servicos;ME;10;Rua Um 100;11999990001;13010010;Campinas;Desenvolvimento de software;matriz;-22.600000;-48.800000;ativa",
            $"{GerarCnpjValido(2)};Empresa Dois;Fantasia Dois;6201500;industria;EPP;20;Rua Dois 200;11999990002;13010011;Campinas;Fabricacao de alimentos;filial;-22.610000;-48.810000;ativa");

        await using var stream = CriarStream(csv);

        var resultado = await service.ImportarAsync(stream, "CSV");

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.TotalRecords);
        Assert.Equal(2, resultado.Inserted);
        Assert.Equal(0, resultado.Updated);
        Assert.Equal(0, resultado.Skipped);
        Assert.Empty(resultado.Errors);
        Assert.Equal("Completed", resultado.Status);
        Assert.Equal(2, await contextMock.Object.Empresas.CountAsync());
    }

    [Fact]
    public async Task ImportarRegistrosAsync_ComCnpjExistente_DeveAtualizarRegistro()
    {
        var contextMock = CreateContextMock();
        var loggerMock = new Mock<ILogger<ImportacaoEmpresasService>>();
        var service = new ImportacaoEmpresasService(contextMock.Object, loggerMock.Object);
        var cnpj = GerarCnpjValido(3);

        contextMock.Object.Empresas.Add(new Domain.Entities.Empresa
        {
            Cnpj = cnpj,
            RazaoSocial = "Empresa Antiga",
            NomeFantasia = "Fantasia Antiga",
            CnaePrincipal = "6201500",
            Setor = Domain.Entities.SetorEmpresa.Servicos,
            Porte = Domain.Entities.PorteEmpresa.Me,
            NumeroFuncionarios = 5,
            Endereco = "Rua Antiga, 10",
            Telefone = "11999990000",
            Cep = "13010010",
            Municipio = "Campinas",
            DescricaoCnae = "Descricao antiga",
            MatrizOuFilial = Domain.Entities.MatrizOuFilialEmpresa.Matriz,
            Latitude = -22.500000m,
            Longitude = -48.700000m,
            SituacaoCadastral = Domain.Entities.SituacaoCadastral.Ativa,
            DataCadastro = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
        await contextMock.Object.SaveChangesAsync();

        var registros = new List<EmpresaImportRecord>
        {
            CriarRegistroValido(cnpj, "Empresa Atualizada")
        };

        var resultado = await service.ImportarRegistrosAsync(registros.ToAsyncEnumerable(), "Teste Update");
        var empresaAtualizada = await contextMock.Object.Empresas.SingleAsync(e => e.Cnpj == cnpj);

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.TotalRecords);
        Assert.Equal(0, resultado.Inserted);
        Assert.Equal(1, resultado.Updated);
        Assert.Equal(0, resultado.Skipped);
        Assert.Empty(resultado.Errors);
        Assert.Equal("Completed", resultado.Status);
        Assert.Equal("Empresa Atualizada", empresaAtualizada.RazaoSocial);
        Assert.Equal("Empresa Atualizada", empresaAtualizada.NomeFantasia);
        Assert.Equal(10, empresaAtualizada.NumeroFuncionarios);
        Assert.Equal(1, await contextMock.Object.Empresas.CountAsync());
    }

    [Fact]
    public async Task ImportarRegistrosAsync_ComCnpjDuplicado_DeveIgnorarDuplicado()
    {
        var contextMock = CreateContextMock();
        var loggerMock = new Mock<ILogger<ImportacaoEmpresasService>>();
        var service = new ImportacaoEmpresasService(contextMock.Object, loggerMock.Object);
        var cnpj = GerarCnpjValido(10);

        var registros = new List<EmpresaImportRecord>
        {
            CriarRegistroValido(cnpj, "Empresa Original"),
            CriarRegistroValido(cnpj, "Empresa Duplicada")
        };

        var resultado = await service.ImportarRegistrosAsync(registros.ToAsyncEnumerable(), "Teste Duplicado");

        Assert.NotNull(resultado);
        Assert.Equal(2, resultado.TotalRecords);
        Assert.Equal(1, resultado.Inserted);
        Assert.Equal(0, resultado.Updated);
        Assert.Equal(1, resultado.Skipped);
        Assert.Equal("CompletedWithErrors", resultado.Status);
        Assert.Contains(resultado.Errors, erro => erro.ErrorType == "DuplicateInBatch");
        Assert.Equal(1, await contextMock.Object.Empresas.CountAsync());
    }

    [Fact]
    public async Task ImportarRegistrosAsync_ComLinhasInvalidas_DeveRegistrarErros()
    {
        var contextMock = CreateContextMock();
        var loggerMock = new Mock<ILogger<ImportacaoEmpresasService>>();
        var service = new ImportacaoEmpresasService(contextMock.Object, loggerMock.Object);

        var registros = new List<EmpresaImportRecord>
        {
            CriarRegistroValido(GerarCnpjValido(20), "Empresa Valida"),
            new()
            {
                RecordId = "linha-invalida-1",
                Cnpj = string.Empty,
                RazaoSocial = "Sem CNPJ",
                Setor = "servicos",
                Porte = "ME",
                SituacaoCadastral = "ativa",
                Endereco = "Rua Sem Cnpj"
            },
            new()
            {
                RecordId = "linha-invalida-2",
                Cnpj = GerarCnpjValido(21),
                RazaoSocial = string.Empty,
                Setor = "servicos",
                Porte = "ME",
                SituacaoCadastral = "ativa",
                Endereco = "Rua Sem Razao"
            }
        };

        var resultado = await service.ImportarRegistrosAsync(registros.ToAsyncEnumerable(), "Teste Invalidos");

        Assert.NotNull(resultado);
        Assert.Equal(3, resultado.TotalRecords);
        Assert.Equal(1, resultado.Inserted);
        Assert.Equal(0, resultado.Updated);
        Assert.Equal(2, resultado.Skipped);
        Assert.Equal("CompletedWithErrors", resultado.Status);
        Assert.Equal(2, resultado.Errors.Count);
        Assert.All(resultado.Errors, erro => Assert.Equal("Validation", erro.Stage));
        Assert.Equal(1, await contextMock.Object.Empresas.CountAsync());
    }

    [Fact]
    public async Task ImportarAsync_ComArquivoVazio_DeveRetornarZeroRegistros()
    {
        var contextMock = CreateContextMock();
        var loggerMock = new Mock<ILogger<ImportacaoEmpresasService>>();
        var service = new ImportacaoEmpresasService(contextMock.Object, loggerMock.Object);

        var csvSomenteCabecalho = "CNPJ;Razao_Social;Nome_Fantasia;CNAE_Principal;Setor;Porte;Numero_Funcionarios;Endereco;Telefone;CEP;Municipio;Descricao_CNAE;Matriz_ou_Filial;Latitude;Longitude;Situacao_Cadastral";
        await using var stream = CriarStream(csvSomenteCabecalho);

        var resultado = await service.ImportarAsync(stream, "CSV");

        Assert.NotNull(resultado);
        Assert.Equal(0, resultado.TotalRecords);
        Assert.Equal(0, resultado.Inserted);
        Assert.Equal(0, resultado.Updated);
        Assert.Equal(0, resultado.Skipped);
        Assert.Empty(resultado.Errors);
        Assert.Equal("Completed", resultado.Status);
        Assert.Equal(0, await contextMock.Object.Empresas.CountAsync());
    }

    [Fact]
    public async Task ImportarRegistrosAsync_ComCamposOpcionaisAusentes_DevePersistirNulos()
    {
        var contextMock = CreateContextMock();
        var loggerMock = new Mock<ILogger<ImportacaoEmpresasService>>();
        var service = new ImportacaoEmpresasService(contextMock.Object, loggerMock.Object);

        var cnpj = GerarCnpjValido(30);
        var registros = new List<EmpresaImportRecord>
        {
            new()
            {
                RecordId = Guid.NewGuid().ToString(),
                Cnpj = cnpj,
                RazaoSocial = "Empresa Sem Complementos",
                NomeFantasia = "Empresa Sem Complementos",
                CnaePrincipal = "6201500",
                Setor = "servicos",
                Porte = "ME",
                NumeroFuncionarios = null,
                Endereco = "Rua Sem Complementos, 100",
                Telefone = string.Empty,
                Cep = string.Empty,
                Municipio = "Campinas",
                DescricaoCnae = "Atividades de consultoria em tecnologia",
                MatrizOuFilial = "matriz",
                Latitude = -22.600000m,
                Longitude = -48.800000m,
                SituacaoCadastral = "ativa",
                FonteOrigem = "Teste",
                DataImportacao = DateTime.UtcNow
            }
        };

        var resultado = await service.ImportarRegistrosAsync(registros.ToAsyncEnumerable(), "Teste Nulls");
        var empresa = await contextMock.Object.Empresas.SingleAsync(e => e.Cnpj == cnpj);

        Assert.Equal("Completed", resultado.Status);
        Assert.Null(empresa.NumeroFuncionarios);
        Assert.Null(empresa.Telefone);
        Assert.Null(empresa.Cep);
    }

    [Fact]
    public async Task ImportarAsync_ComCsvEditadoNoExcel_DeveAceitarCnpjApostrofadoDecimalComVirgulaEDataSemSeparador()
    {
        var contextMock = CreateContextMock();
        var loggerMock = new Mock<ILogger<ImportacaoEmpresasService>>();
        var service = new ImportacaoEmpresasService(contextMock.Object, loggerMock.Object);

        var cnpj = GerarCnpjValido(1);
        var csv = string.Join(Environment.NewLine,
            "CNPJ;Razao_Social;Nome_Fantasia;CNAE_Principal;Setor;Porte;Numero_Funcionarios;Endereco;Telefone;CEP;Municipio;Descricao_CNAE;Matriz_ou_Filial;Latitude;Longitude;Situacao_Cadastral;Data_Importacao;Fonte_Origem",
            $"'{cnpj};Empresa Excel;Empresa Excel;6201500;servicos;ME;10;Rua Um 100;;13010010;Campinas;Desenvolvimento de software;Matriz;-22,60245;-48,79995;ativa;07/05/2026  113010;Exportacao_Cadastro_Atual");

        await using var stream = CriarStream(csv);

        var resultado = await service.ImportarAsync(stream, "CSV");

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.TotalRecords);
        Assert.Equal(1, resultado.Inserted);
        Assert.Equal(0, resultado.Updated);
        Assert.Equal(0, resultado.Skipped);
        Assert.Empty(resultado.Errors);
        Assert.Equal("Completed", resultado.Status);
        var empresa = await contextMock.Object.Empresas.SingleAsync(e => e.Cnpj == cnpj);
        Assert.Equal(-22.60245m, empresa.Latitude);
        Assert.Equal(-48.79995m, empresa.Longitude);
    }

    [Fact]
    public async Task ImportarRegistrosAsync_ConverteDataImportacaoParaUtc()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        var context = new AppDbContext(options);
        var logger = new Mock<ILogger<ImportacaoEmpresasService>>().Object;

        var service = new ImportacaoEmpresasService(context, logger);
        var registros = new List<EmpresaImportRecord>
        {
            new EmpresaImportRecord
            {
                RecordId = "1",
                Cnpj = "12345678000195",
                RazaoSocial = "Empresa Teste",
                DataImportacao = new DateTime(2026, 5, 10, 12, 0, 0, DateTimeKind.Local) // Data local
            }
        }.ToAsyncEnumerable();

        // Act
        var resultado = await service.ImportarRegistrosAsync(registros, "Teste", CancellationToken.None);

        // Assert
        Assert.Equal(1, resultado.Inserted);
        var empresa = context.Empresas.FirstOrDefault(e => e.Cnpj == "12345678000195");
        Assert.NotNull(empresa);
        Assert.Equal(DateTimeKind.Utc, empresa.DataCadastro.Kind);
        Assert.Equal(new DateTime(2026, 5, 10, 15, 0, 0, DateTimeKind.Utc), empresa.DataCadastro); // Convertido para UTC
    }

    [Fact]
    public async Task ImportarPontosInstitucionais_Deve_Normalizar_Coordenadas()
    {
        // Arrange
        var csvContent = "Id;Nome;Tipo;Descricao;Endereco;Latitude;Longitude\n" +
                     "1;Ponto A;1;Descricao A;Endereco A;-91;45\n" +
                     "2;Ponto B;2;Descricao B;Endereco B;30;181";

        var fileMock = new Mock<IFormFile>();
        var content = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
        fileMock.Setup(_ => _.OpenReadStream()).Returns(content);
        fileMock.Setup(_ => _.FileName).Returns("pontos.csv");
        fileMock.Setup(_ => _.Length).Returns(content.Length);

        var contextMock = new Mock<AppDbContext>();
        var loggerMock = new Mock<ILogger<ImportacaoPontosInstitucionaisController>>();

        var controller = new ImportacaoPontosInstitucionaisController(contextMock.Object, loggerMock.Object);

        // Act
        var request = new ImportacaoPontosInstitucionaisRequest
        {
            File = fileMock.Object
        };
        var result = await controller.ImportarPontosInstitucionais(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var importResult = Assert.IsType<PontoInstitucionalImportResult>(okResult.Value);
        Assert.Equal(2, importResult.TotalRecords);
        Assert.Equal(2, importResult.Skipped); // Coordenadas inválidas devem ser ignoradas
    }

    private static Mock<AppDbContext> CreateContextMock()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Mock<AppDbContext>(options) { CallBase = true };
    }

    private static EmpresaImportRecord CriarRegistroValido(string cnpj, string razaoSocial)
    {
        return new EmpresaImportRecord
        {
            RecordId = Guid.NewGuid().ToString(),
            Cnpj = cnpj,
            RazaoSocial = razaoSocial,
            NomeFantasia = razaoSocial,
            CnaePrincipal = "6201500",
            Setor = "servicos",
            Porte = "ME",
            NumeroFuncionarios = 10,
            Endereco = $"Rua {razaoSocial}, 100",
            Telefone = "11999994444",
            Cep = "13010015",
            Municipio = "Campinas",
            DescricaoCnae = "Atividades de consultoria em tecnologia",
            MatrizOuFilial = "matriz",
            Latitude = -22.600000m,
            Longitude = -48.800000m,
            SituacaoCadastral = "ativa",
            FonteOrigem = "Teste",
            DataImportacao = DateTime.UtcNow
        };
    }

    private static MemoryStream CriarStream(string content)
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(content));
    }

    private static string GerarCnpjValido(int sequencia)
    {
        var raiz = $"12345678{sequencia:D4}";
        var primeiroDigito = CalcularDigito(raiz, new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 });
        var segundoDigito = CalcularDigito(raiz + primeiroDigito, new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 });
        return raiz + primeiroDigito + segundoDigito;
    }

    private static int CalcularDigito(string baseCnpj, int[] pesos)
    {
        var soma = 0;

        for (var i = 0; i < pesos.Length; i++)
        {
            soma += (baseCnpj[i] - '0') * pesos[i];
        }

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}

public static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
    {
        foreach (var item in source)
        {
            yield return await Task.FromResult(item);
        }
    }
}
