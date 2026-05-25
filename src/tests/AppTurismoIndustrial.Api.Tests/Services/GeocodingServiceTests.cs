using AppTurismoIndustrial.Api.Features.Geocoding;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AppTurismoIndustrial.Api.Tests.Services;

public class GeocodingServiceTests
{
    [Fact]
    public async Task Geocode_ComCepValido_UsaViaCepEDepoisNominatim()
    {
        var viaCep = new Mock<IViaCepClient>();
        viaCep.Setup(v => v.ResolveAsync("CEP 18681420", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ViaCepAddress(
                Cep: "18681-420",
                Logradouro: "Rua Francisco Marins",
                Bairro: "Nucleo Habitacional Joao Zillo III",
                Localidade: "Lencois Paulista",
                Uf: "SP"));

        string? capturedEndereco = null;
        string? capturedCidade = null;
        string? capturedEstado = null;
        var provider = new Mock<IGeocodingProvider>();
        provider.SetupGet(p => p.ProviderName).Returns("MockNominatim");
        provider.Setup(p => p.GeocodeAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback<string, string?, string?, CancellationToken>((e, c, s, _) =>
            {
                capturedEndereco = e;
                capturedCidade = c;
                capturedEstado = s;
            })
            .ReturnsAsync(new GeocodeResult
            {
                Latitude = -22.60m,
                Longitude = -48.80m,
                Provider = "MockNominatim",
                Accuracy = "house",
                ObtainedAt = DateTime.UtcNow,
            });

        var service = new GeocodingService(
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<GeocodingService>.Instance,
            provider.Object,
            viaCep.Object);

        var result = await service.GeocodeAsync("CEP 18681420");

        Assert.NotNull(result);
        Assert.Equal(-22.60m, result!.Latitude);
        Assert.Equal(-48.80m, result.Longitude);
        // O Provider deve ter sido chamado com o endereco enriquecido pelo ViaCEP,
        // nao com "CEP 18681420".
        Assert.Equal("Rua Francisco Marins, Nucleo Habitacional Joao Zillo III, Lencois Paulista", capturedEndereco);
        Assert.Equal("Lencois Paulista", capturedCidade);
        Assert.Equal("SP", capturedEstado);
    }

    [Fact]
    public async Task Geocode_ComCepQueViaCepNaoResolve_FallbackParaNominatimDireto()
    {
        var viaCep = new Mock<IViaCepClient>();
        viaCep.Setup(v => v.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ViaCepAddress?)null);

        string? capturedEndereco = null;
        var provider = new Mock<IGeocodingProvider>();
        provider.SetupGet(p => p.ProviderName).Returns("MockNominatim");
        provider.Setup(p => p.GeocodeAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .Callback<string, string?, string?, CancellationToken>((e, _, _, _) => capturedEndereco = e)
            .ReturnsAsync(new GeocodeResult { Latitude = -22m, Longitude = -49m, Provider = "MockNominatim", Accuracy = "approximate", ObtainedAt = DateTime.UtcNow });

        var service = new GeocodingService(
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<GeocodingService>.Instance,
            provider.Object,
            viaCep.Object);

        var result = await service.GeocodeAsync("99999999");

        Assert.NotNull(result);
        // Quando ViaCEP nao resolve, a query original "99999999" eh passada ao
        // Nominatim como esta (comportamento legacy).
        Assert.Equal("99999999", capturedEndereco);
    }

    [Fact]
    public async Task Geocode_ComEnderecoTextual_NaoChamaViaCep()
    {
        var viaCep = new Mock<IViaCepClient>(MockBehavior.Strict);
        // MockBehavior.Strict: chamada inesperada ao ViaCEP quebra o teste.

        var provider = new Mock<IGeocodingProvider>();
        provider.SetupGet(p => p.ProviderName).Returns("MockNominatim");
        provider.Setup(p => p.GeocodeAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GeocodeResult { Latitude = -22m, Longitude = -49m, Provider = "MockNominatim", Accuracy = "approximate", ObtainedAt = DateTime.UtcNow });

        var service = new GeocodingService(
            new MemoryCache(new MemoryCacheOptions()),
            NullLogger<GeocodingService>.Instance,
            provider.Object,
            viaCep.Object);

        var result = await service.GeocodeAsync("Av Paulista, 1578, Sao Paulo");

        Assert.NotNull(result);
        viaCep.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("18681420", true)]
    [InlineData("18681-420", true)]
    [InlineData("CEP 18681420", true)]
    [InlineData("CEP 18681-420", true)]
    [InlineData("  CEP  18681-420  ", true)]
    [InlineData("Av Paulista 1578", false)]
    [InlineData("1234", false)]
    [InlineData("1234567", false)]
    [InlineData("123456789", false)]
    public void CepRegex_CasaCepsBrasileirosEmDiversasFormas(string input, bool expectedMatch)
    {
        Assert.Equal(expectedMatch, ViaCepClient.CepRegex.IsMatch(input));
    }
}
