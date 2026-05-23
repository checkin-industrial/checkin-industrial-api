using AppTurismoIndustrial.Api.Application.DTOs;
using AppTurismoIndustrial.Api.Application.Services;
using AppTurismoIndustrial.Api.Domain.Entities;
using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AppTurismoIndustrial.Api.Tests.Services;

public class EmpresaServiceTests
{
    [Fact]
    public async Task CriarEmpresa_DeveCriarComSucesso()
    {
        // Arrange
        var contextMock = CreateContextMock();
        var service = new EmpresaService(contextMock.Object);
        var dto = CriarDtoEmpresa("12345678000195");

        // Act
        var (empresa, cnpjDuplicado) = await service.CriarAsync(dto);

        // Assert
        Assert.False(cnpjDuplicado);
        Assert.NotNull(empresa);
        Assert.Equal(dto.Cnpj, empresa!.Cnpj);
        Assert.Equal(dto.RazaoSocial, empresa.RazaoSocial);

        var totalEmpresas = await contextMock.Object.Empresas.CountAsync();
        Assert.Equal(1, totalEmpresas);

        contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarEmpresa_ComCnpjDuplicado_DeveImpedirCriacao()
    {
        // Arrange
        var contextMock = CreateContextMock();
        var service = new EmpresaService(contextMock.Object);

        contextMock.Object.Empresas.Add(new Empresa
        {
            Cnpj = "12345678000195",
            RazaoSocial = "Empresa Existente",
            NomeFantasia = "Existente",
            CnaePrincipal = "6201500",
            Setor = SetorEmpresa.Servicos,
            Porte = PorteEmpresa.Me,
            NumeroFuncionarios = 10,
            Endereco = "Rua A, 100",
            Telefone = "11999990000",
            Cep = "13010010",
            Municipio = "Campinas",
            DescricaoCnae = "Desenvolvimento de software",
            MatrizOuFilial = MatrizOuFilialEmpresa.Matriz,
            Latitude = -22.6m,
            Longitude = -48.8m,
            SituacaoCadastral = SituacaoCadastral.Ativa,
            DataCadastro = DateTime.UtcNow
        });
        await contextMock.Object.SaveChangesAsync();

        var dto = CriarDtoEmpresa("12345678000195");

        // Act
        var (empresa, cnpjDuplicado) = await service.CriarAsync(dto);

        // Assert
        Assert.True(cnpjDuplicado);
        Assert.Null(empresa);

        var totalEmpresas = await contextMock.Object.Empresas.CountAsync();
        Assert.Equal(1, totalEmpresas);
    }

    [Fact]
    public async Task ObterPorId_DeveRetornarEmpresaQuandoExistir()
    {
        // Arrange
        var contextMock = CreateContextMock();
        var service = new EmpresaService(contextMock.Object);

        var id = Guid.NewGuid();
        contextMock.Object.Empresas.Add(new Empresa
        {
            Id = id,
            Cnpj = "98765432000188",
            RazaoSocial = "Empresa Teste",
            NomeFantasia = "Teste",
            CnaePrincipal = "6201500",
            Setor = SetorEmpresa.Servicos,
            Porte = PorteEmpresa.Me,
            NumeroFuncionarios = 25,
            Endereco = "Rua B, 200",
            Telefone = "11999991111",
            Cep = "13010011",
            Municipio = "Campinas",
            DescricaoCnae = "Desenvolvimento de software sob encomenda",
            MatrizOuFilial = MatrizOuFilialEmpresa.Filial,
            Latitude = -22.65m,
            Longitude = -48.75m,
            SituacaoCadastral = SituacaoCadastral.Ativa,
            DataCadastro = DateTime.UtcNow
        });
        await contextMock.Object.SaveChangesAsync();

        // Act
        var resultado = await service.ObterPorIdAsync(id);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(id, resultado!.Id);
        Assert.Equal("98765432000188", resultado.Cnpj);
        Assert.Equal("Empresa Teste", resultado.RazaoSocial);
    }

    [Fact]
    public async Task ObterPorId_ComCamposOpcionaisNulos_DeveNormalizarResposta()
    {
        var contextMock = CreateContextMock();
        var service = new EmpresaService(contextMock.Object);

        var id = Guid.NewGuid();
        contextMock.Object.Empresas.Add(new Empresa
        {
            Id = id,
            Cnpj = "11111111000191",
            RazaoSocial = "Empresa Sem Contato",
            NomeFantasia = "Sem Contato",
            CnaePrincipal = "6201500",
            Setor = SetorEmpresa.Servicos,
            Porte = PorteEmpresa.Me,
            NumeroFuncionarios = null,
            Endereco = "Rua B, 200",
            Telefone = null,
            Cep = null,
            Municipio = "Campinas",
            DescricaoCnae = "Desenvolvimento de software sob encomenda",
            MatrizOuFilial = MatrizOuFilialEmpresa.Filial,
            Latitude = -22.65m,
            Longitude = -48.75m,
            SituacaoCadastral = SituacaoCadastral.Ativa,
            DataCadastro = DateTime.UtcNow
        });
        await contextMock.Object.SaveChangesAsync();

        var resultado = await service.ObterPorIdAsync(id);

        Assert.NotNull(resultado);
        Assert.Equal(string.Empty, resultado!.Telefone);
        Assert.Equal(string.Empty, resultado.Cep);
        Assert.Equal(0, resultado.NumeroFuncionarios);
    }

    private static Mock<AppDbContext> CreateContextMock()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new Mock<AppDbContext>(options) { CallBase = true };
    }

    private static DTOEmpresaCriar CriarDtoEmpresa(string cnpj)
    {
        return new DTOEmpresaCriar
        {
            Cnpj = cnpj,
            RazaoSocial = "Empresa Nova",
            NomeFantasia = "Nova",
            CnaePrincipal = "6201500",
            Setor = SetorEmpresa.Servicos,
            Porte = PorteEmpresa.Me,
            NumeroFuncionarios = 15,
            Endereco = "Rua Central, 123",
            Telefone = "11988887777",
            Cep = "13010012",
            Municipio = "Campinas",
            DescricaoCnae = "Consultoria em tecnologia",
            MatrizOuFilial = MatrizOuFilialEmpresa.Matriz,
            Latitude = -22.63m,
            Longitude = -48.79m,
            SituacaoCadastral = SituacaoCadastral.Ativa,
            DataCadastro = DateTime.UtcNow
        };
    }
}
