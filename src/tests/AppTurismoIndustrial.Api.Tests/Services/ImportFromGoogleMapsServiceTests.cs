using AppTurismoIndustrial.Api.Features.Geocoding;
using AppTurismoIndustrial.Api.Infrastructure.Persistence;
using AppTurismoIndustrial.Api.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace AppTurismoIndustrial.Api.Tests.Services;

public class ImportFromGoogleMapsServiceTests
{
    [Fact]
    public async Task Import_DeveCriarEmpresasComAtivoFalse()
    {
        var ctx = CreateContext();
        var geocoding = MockGeocoding(-22.5m, -49.0m);
        var places = MockPlacesClient(
            new GooglePlace("PLACE-1", "Empresa A", "Rua A, 100", -22.5, -49.0, new[] { "store" }, "(14) 3000-0000", null, "https://a.com"),
            new GooglePlace("PLACE-2", "Empresa B", "Rua B, 200", -22.51, -49.01, new[] { "store" }, null, null, null));
        var service = CreateService(ctx, geocoding.Object, places.Object);

        var result = await service.ImportAsync(new DTOImportFromGoogleMapsRequest
        {
            Cep = "17000000",
            RaioMetros = 5000,
            Tipo = "loja",
        });

        Assert.Equal(2, result.Encontrados);
        Assert.Equal(2, result.Criados);
        Assert.Equal(0, result.Atualizados);
        Assert.Equal(0, result.Ignorados);

        var empresas = await ctx.Empresas.AsNoTracking().ToListAsync();
        Assert.Equal(2, empresas.Count);
        Assert.All(empresas, e => Assert.False(e.Ativo));
        Assert.All(empresas, e => Assert.Null(e.Cnpj));
        Assert.Contains(empresas, e => e.GooglePlaceId == "PLACE-1" && e.Telefone == "(14) 3000-0000");

        var logs = await ctx.GoogleMapsImportLogs.AsNoTracking().ToListAsync();
        Assert.Single(logs);
        Assert.Equal(2, logs[0].EmpresasCriadas);
    }

    [Fact]
    public async Task Import_DeveEnriquecerEmpresaExistentePorGooglePlaceId()
    {
        var ctx = CreateContext();
        ctx.Empresas.Add(new Empresa
        {
            Id = Guid.NewGuid(),
            GooglePlaceId = "PLACE-1",
            RazaoSocial = "Existente",
            NomeFantasia = "Existente",
            CnaePrincipal = "0000000",
            DescricaoCnae = "antigo",
            Setor = SetorEmpresa.Comercio,
            Porte = PorteEmpresa.Me,
            Endereco = "(sem endereco)",
            Municipio = "Importado",
            MatrizOuFilial = MatrizOuFilialEmpresa.Matriz,
            Latitude = -22.5m,
            Longitude = -49.0m,
            SituacaoCadastral = SituacaoCadastral.Ativa,
            Ativo = false,
            Telefone = null,
        });
        await ctx.SaveChangesAsync();

        var geocoding = MockGeocoding(-22.5m, -49.0m);
        var places = MockPlacesClient(
            new GooglePlace("PLACE-1", "Empresa A", "Rua A, 100 - novo", -22.5, -49.0, new[] { "store" }, "(14) 9999-9999", null, null));
        var service = CreateService(ctx, geocoding.Object, places.Object);

        var result = await service.ImportAsync(new DTOImportFromGoogleMapsRequest
        {
            Cep = "17000000",
            RaioMetros = 5000,
            Tipo = "loja",
        });

        Assert.Equal(1, result.Atualizados);
        Assert.Equal(0, result.Criados);

        var existente = await ctx.Empresas.AsNoTracking().FirstAsync(e => e.GooglePlaceId == "PLACE-1");
        Assert.Equal("(14) 9999-9999", existente.Telefone);
        Assert.Equal("Rua A, 100 - novo", existente.Endereco);
    }

    [Fact]
    public async Task Import_DeveIgnorarQuandoEmpresaJaTemDadosCompletos()
    {
        var ctx = CreateContext();
        ctx.Empresas.Add(new Empresa
        {
            Id = Guid.NewGuid(),
            GooglePlaceId = "PLACE-1",
            RazaoSocial = "Existente",
            NomeFantasia = "Existente",
            CnaePrincipal = "0000000",
            DescricaoCnae = "ok",
            Setor = SetorEmpresa.Comercio,
            Porte = PorteEmpresa.Me,
            Endereco = "Rua existente, 50",
            Telefone = "(14) 1111-1111",
            Municipio = "Bauru",
            MatrizOuFilial = MatrizOuFilialEmpresa.Matriz,
            Latitude = -22.5m,
            Longitude = -49.0m,
            SituacaoCadastral = SituacaoCadastral.Ativa,
            Ativo = true,
        });
        await ctx.SaveChangesAsync();

        var geocoding = MockGeocoding(-22.5m, -49.0m);
        var places = MockPlacesClient(
            new GooglePlace("PLACE-1", "Empresa A", "Rua Google, 100", -22.5, -49.0, new[] { "store" }, "(14) 9999-9999", null, null));
        var service = CreateService(ctx, geocoding.Object, places.Object);

        var result = await service.ImportAsync(new DTOImportFromGoogleMapsRequest
        {
            Cep = "17000000",
            RaioMetros = 5000,
            Tipo = "loja",
        });

        Assert.Equal(1, result.Ignorados);
        // Dados preservados (nunca sobrescreve).
        var existente = await ctx.Empresas.AsNoTracking().FirstAsync(e => e.GooglePlaceId == "PLACE-1");
        Assert.Equal("(14) 1111-1111", existente.Telefone);
        Assert.Equal("Rua existente, 50", existente.Endereco);
    }

    [Fact]
    public async Task Import_DeveRejeitarRaioAcimaDoMaximo()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx, MockGeocoding(-22.5m, -49.0m).Object, MockPlacesClient().Object,
            options => options.MaxRaioMetros = 5000);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.ImportAsync(new DTOImportFromGoogleMapsRequest
            {
                Cep = "17000000",
                RaioMetros = 9999,
                Tipo = "loja",
            }));
    }

    [Fact]
    public async Task Import_DeveRejeitarTipoNaoSuportado()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx, MockGeocoding(-22.5m, -49.0m).Object, MockPlacesClient().Object);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.ImportAsync(new DTOImportFromGoogleMapsRequest
            {
                Cep = "17000000",
                RaioMetros = 5000,
                Tipo = "tipo-inexistente",
            }));
    }

    [Fact]
    public async Task Import_DeveRejeitarCoordenadaForaDaRegiao()
    {
        var ctx = CreateContext();
        var service = CreateService(ctx, MockGeocoding(-30.0m, -60.0m).Object, MockPlacesClient().Object,
            options => options.AllowedRegion = new RegionBounds
            {
                LatMin = -23.0,
                LatMax = -22.0,
                LngMin = -50.0,
                LngMax = -48.0,
            });

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.ImportAsync(new DTOImportFromGoogleMapsRequest
            {
                Cep = "17000000",
                RaioMetros = 5000,
                Tipo = "loja",
            }));
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Mock<IGeocodingService> MockGeocoding(decimal lat, decimal lng)
    {
        var mock = new Mock<IGeocodingService>();
        mock.Setup(g => g.GeocodeAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeocodeResult { Latitude = lat, Longitude = lng });
        return mock;
    }

    private static Mock<IGooglePlacesClient> MockPlacesClient(params GooglePlace[] places)
    {
        var mock = new Mock<IGooglePlacesClient>();
        mock.Setup(c => c.NearbySearchAsync(
                It.IsAny<double>(), It.IsAny<double>(), It.IsAny<int>(),
                It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GooglePlacesNearbyResponse(places, "{}"));
        return mock;
    }

    private static ImportFromGoogleMapsService CreateService(
        AppDbContext ctx,
        IGeocodingService geocoding,
        IGooglePlacesClient places,
        Action<GoogleMapsOptions>? configure = null)
    {
        var options = new GoogleMapsOptions();
        configure?.Invoke(options);
        return new ImportFromGoogleMapsService(
            ctx, geocoding, places,
            Options.Create(options),
            NullLogger<ImportFromGoogleMapsService>.Instance);
    }
}
