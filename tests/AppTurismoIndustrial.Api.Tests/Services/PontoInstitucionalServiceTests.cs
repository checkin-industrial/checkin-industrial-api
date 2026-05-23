using AppTurismoIndustrial.Api.Application.Queries;
using AppTurismoIndustrial.Api.Application.Services;
using AppTurismoIndustrial.Api.Domain.Entities;
using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AppTurismoIndustrial.Api.Tests.Services;

public class PontoInstitucionalServiceTests
{
    [Fact]
    public async Task ObterPorIdAsync_ComCamposOpcionaisNulos_DeveNormalizarResposta()
    {
        var context = CreateContext();
        var queryMock = new Mock<IPontoInstitucionalQuery>();

        var id = Guid.NewGuid();
        context.PontosInstitucionais.Add(new PontoInstitucional
        {
            Id = id,
            Nome = "Ponto Sem Contato",
            Tipo = TipoPontoInstitucional.Educacao,
            Descricao = "Descricao teste",
            Endereco = "Rua Teste, 100",
            Latitude = -22.600000m,
            Longitude = -48.800000m,
            AtividadesDisponiveis = null,
            EquipeGestao = null,
            ContatoNome = null,
            ContatoTelefone = null,
            ContatoEmail = null,
            CorMarcador = null,
            IconeMarcador = null,
            OrdemExibicao = null,
            Ativo = null,
        });
        await context.SaveChangesAsync();

        var service = new PontoInstitucionalService(queryMock.Object, context);

        var resultado = await service.ObterPorIdAsync(id);

        Assert.NotNull(resultado);
        Assert.Equal(string.Empty, resultado!.AtividadesDisponiveis);
        Assert.Equal(string.Empty, resultado.EquipeGestao);
        Assert.Equal(string.Empty, resultado.ContatoNome);
        Assert.Equal(string.Empty, resultado.ContatoTelefone);
        Assert.Equal(string.Empty, resultado.ContatoEmail);
        Assert.Equal("#0d9488", resultado.CorMarcador);
        Assert.Equal("institucional", resultado.IconeMarcador);
        Assert.Equal(0, resultado.OrdemExibicao);
        Assert.True(resultado.Ativo);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}