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
    public async Task Import_DeveCriarCandidatesPendentes()
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
            RaioMetros = 800,
            Tipo = "loja",
        });

        Assert.Equal(2, result.Encontrados);
        Assert.Equal(2, result.CandidatesCriados);
        Assert.Equal(0, result.CandidatesAtualizados);
        Assert.Equal(0, result.CandidatesIgnorados);

        // Empresas tabela continua vazia — fluxo novo nao cria empresa direto.
        var empresas = await ctx.Empresas.AsNoTracking().ToListAsync();
        Assert.Empty(empresas);

        // Candidates tabela recebe os 2 itens, todos com status Pendente em todos os destinos.
        var candidates = await ctx.GoogleMapsImportCandidates.AsNoTracking().ToListAsync();
        Assert.Equal(2, candidates.Count);
        Assert.All(candidates, c => Assert.Equal(CandidatePromotionStatus.Pendente, c.EmpresaStatus));
        Assert.All(candidates, c => Assert.Equal(CandidatePromotionStatus.Pendente, c.PontoStatus));
        Assert.All(candidates, c => Assert.Equal(CandidatePromotionStatus.Pendente, c.TelefoneStatus));
        Assert.All(candidates, c => Assert.Null(c.EmpresaId));
        Assert.All(candidates, c => Assert.Equal("17000000", c.CepOrigem));
        Assert.Contains(candidates, c => c.GooglePlaceId == "PLACE-1" && c.Telefone == "(14) 3000-0000");

        var logs = await ctx.GoogleMapsImportLogs.AsNoTracking().ToListAsync();
        Assert.Single(logs);
        Assert.Equal(2, logs[0].EmpresasCriadas);  // contador legacy reusado p/ candidates
    }

    [Fact]
    public async Task Import_DeveEnriquecerCandidateExistentePorGooglePlaceId()
    {
        var ctx = CreateContext();
        var logExistente = new GoogleMapsImportLog
        {
            Id = Guid.NewGuid(),
            Cep = "17000000",
            RaioMetros = 800,
            Tipo = "loja",
            LatitudeOrigem = -22.5m,
            LongitudeOrigem = -49.0m,
        };
        ctx.GoogleMapsImportLogs.Add(logExistente);
        ctx.GoogleMapsImportCandidates.Add(new GoogleMapsImportCandidate
        {
            Id = Guid.NewGuid(),
            GoogleMapsImportLogId = logExistente.Id,
            GooglePlaceId = "PLACE-1",
            Nome = "Empresa A",
            FormattedAddress = null,  // vazio - vai ser enriquecido
            Latitude = -22.5m,
            Longitude = -49.0m,
            Telefone = null,  // vazio - vai ser enriquecido
            TypesJson = "[]",
            CepOrigem = "17000000",
        });
        await ctx.SaveChangesAsync();

        var geocoding = MockGeocoding(-22.5m, -49.0m);
        var places = MockPlacesClient(
            new GooglePlace("PLACE-1", "Empresa A", "Rua A, 100 - novo", -22.5, -49.0, new[] { "store" }, "(14) 9999-9999", null, null));
        var service = CreateService(ctx, geocoding.Object, places.Object);

        var result = await service.ImportAsync(new DTOImportFromGoogleMapsRequest
        {
            Cep = "17000000",
            RaioMetros = 800,
            Tipo = "loja",
        });

        Assert.Equal(1, result.CandidatesAtualizados);
        Assert.Equal(0, result.CandidatesCriados);

        var existente = await ctx.GoogleMapsImportCandidates.AsNoTracking().FirstAsync(c => c.GooglePlaceId == "PLACE-1");
        Assert.Equal("(14) 9999-9999", existente.Telefone);
        Assert.Equal("Rua A, 100 - novo", existente.FormattedAddress);
    }

    [Fact]
    public async Task Import_DeveIgnorarQuandoCandidateJaTemDadosCompletos()
    {
        var ctx = CreateContext();
        var logExistente = new GoogleMapsImportLog
        {
            Id = Guid.NewGuid(),
            Cep = "17000000",
            RaioMetros = 800,
            Tipo = "loja",
            LatitudeOrigem = -22.5m,
            LongitudeOrigem = -49.0m,
        };
        ctx.GoogleMapsImportLogs.Add(logExistente);
        // Types JSON pre-existente identico ao que vamos retornar do Google,
        // garantindo que EnriquecerCandidate nao detecte mudancas.
        var typesJsonExistente = System.Text.Json.JsonSerializer.Serialize(new[] { "store" });
        ctx.GoogleMapsImportCandidates.Add(new GoogleMapsImportCandidate
        {
            Id = Guid.NewGuid(),
            GoogleMapsImportLogId = logExistente.Id,
            GooglePlaceId = "PLACE-1",
            Nome = "Empresa A",
            FormattedAddress = "Rua existente, 50",
            Latitude = -22.5m,
            Longitude = -49.0m,
            Telefone = "(14) 1111-1111",
            TypesJson = typesJsonExistente,
            CepOrigem = "17000000",
        });
        await ctx.SaveChangesAsync();

        var geocoding = MockGeocoding(-22.5m, -49.0m);
        var places = MockPlacesClient(
            new GooglePlace("PLACE-1", "Empresa A", "Rua Google, 100", -22.5, -49.0, new[] { "store" }, "(14) 9999-9999", null, null));
        var service = CreateService(ctx, geocoding.Object, places.Object);

        var result = await service.ImportAsync(new DTOImportFromGoogleMapsRequest
        {
            Cep = "17000000",
            RaioMetros = 800,
            Tipo = "loja",
        });

        Assert.Equal(1, result.CandidatesIgnorados);
        // Dados preservados (nunca sobrescreve).
        var existente = await ctx.GoogleMapsImportCandidates.AsNoTracking().FirstAsync(c => c.GooglePlaceId == "PLACE-1");
        Assert.Equal("(14) 1111-1111", existente.Telefone);
        Assert.Equal("Rua existente, 50", existente.FormattedAddress);
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
                RaioMetros = 800,
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
                RaioMetros = 800,
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
