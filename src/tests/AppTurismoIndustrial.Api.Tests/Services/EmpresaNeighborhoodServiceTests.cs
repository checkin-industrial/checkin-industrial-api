

using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AppTurismoIndustrial.Api.Tests.Services;

public class EmpresaNeighborhoodServiceTests
{
    [Fact]
    public async Task ObterVizinhancaAsync_DeveRetornarEmpresasDentroDoRaioOrdenadasPorDistancia()
    {
        var context = CreateContext();

        var empresaBaseId = Guid.NewGuid();
        context.Empresas.AddRange(
            CreateEmpresa(empresaBaseId, "Base Industrial", "6201500", SetorEmpresa.Industria, -22.602177m, -48.800792m),
            CreateEmpresa(Guid.NewGuid(), "Vizinha Muito Proxima", "6201500", SetorEmpresa.Industria, -22.603000m, -48.801000m),
            CreateEmpresa(Guid.NewGuid(), "Vizinha Proxima", "4711301", SetorEmpresa.Comercio, -22.610000m, -48.806000m),
            CreateEmpresa(Guid.NewGuid(), "Fora Do Raio", "6201500", SetorEmpresa.Industria, -22.700000m, -48.900000m));

        await context.SaveChangesAsync();

        var service = new EmpresaNeighborhoodService(context);

        var result = await service.ObterVizinhancaAsync(empresaBaseId, 5000, 20);

        Assert.NotNull(result);
        Assert.Equal("Base Industrial", result!.EmpresaBase.NomeFantasia);
        Assert.Equal(2, result.EmpresasProximas.Count);
        Assert.Equal("Vizinha Muito Proxima", result.EmpresasProximas[0].NomeFantasia);
        Assert.True(result.EmpresasProximas[0].MesmoCnae);
        Assert.True(result.EmpresasProximas[0].MesmoSetor);
        Assert.Equal("Vizinha Proxima", result.EmpresasProximas[1].NomeFantasia);
        Assert.False(result.EmpresasProximas[1].MesmoCnae);
    }

    [Fact]
    public async Task ObterVizinhancaAsync_QuandoEmpresaNaoExistir_DeveRetornarNulo()
    {
        var context = CreateContext();
        var service = new EmpresaNeighborhoodService(context);

        var result = await service.ObterVizinhancaAsync(Guid.NewGuid(), 5000, 20);

        Assert.Null(result);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static Empresa CreateEmpresa(
        Guid id,
        string nomeFantasia,
        string cnae,
        SetorEmpresa setor,
        decimal latitude,
        decimal longitude)
    {
        return new Empresa
        {
            Id = id,
            Cnpj = $"{Random.Shared.NextInt64(10_000_000_000_000L, 99_999_999_999_999L)}",
            RazaoSocial = nomeFantasia,
            NomeFantasia = nomeFantasia,
            CnaePrincipal = cnae,
            Setor = setor,
            Porte = PorteEmpresa.Me,
            NumeroFuncionarios = 40,
            Endereco = "Rua Teste, 100",
            Telefone = "14999990000",
            Cep = "18680000",
            Municipio = "Lencois Paulista",
            DescricaoCnae = "Descricao teste",
            MatrizOuFilial = MatrizOuFilialEmpresa.Matriz,
            Latitude = latitude,
            Longitude = longitude,
            SituacaoCadastral = SituacaoCadastral.Ativa,
            DataCadastro = DateTime.UtcNow
        };
    }
}