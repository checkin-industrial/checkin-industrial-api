using System.Text.Encodings.Web;
using AppTurismoIndustrial.Api.Shared.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace AppTurismoIndustrial.Api.Tests.Shared.Auth;

public class ApiKeyAuthenticationHandlerTests
{
    private const string ValidKey = "test-api-key-1234567890abcdef";

    [Fact]
    public async Task SemApiKeyConfigurada_RetornaNoResult()
    {
        var handler = await CreateHandlerAsync(configuredKey: string.Empty, header: ValidKey);
        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.False(result.Failure is not null, "NoResult nao deve ter Failure setado.");
        Assert.Null(result.Principal);
    }

    [Fact]
    public async Task SemHeader_RetornaNoResult()
    {
        var handler = await CreateHandlerAsync(configuredKey: ValidKey, header: null);
        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Null(result.Failure);
        Assert.Null(result.Principal);
    }

    [Fact]
    public async Task HeaderVazio_RetornaNoResult()
    {
        var handler = await CreateHandlerAsync(configuredKey: ValidKey, header: "");
        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Null(result.Failure);
    }

    [Fact]
    public async Task HeaderMultiploValor_RetornaFail()
    {
        var handler = await CreateHandlerAsync(
            configuredKey: ValidKey,
            headerValues: new[] { ValidKey, "outra-coisa" });
        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
        Assert.Contains("exatamente 1 valor", result.Failure!.Message);
    }

    [Fact]
    public async Task ChaveInvalida_RetornaFail()
    {
        var handler = await CreateHandlerAsync(configuredKey: ValidKey, header: "chave-errada");
        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
        Assert.Equal("API Key invalida.", result.Failure!.Message);
    }

    [Fact]
    public async Task ChaveValida_RetornaSuccess()
    {
        var handler = await CreateHandlerAsync(configuredKey: ValidKey, header: ValidKey);
        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Principal);
        Assert.True(result.Principal!.Identity?.IsAuthenticated);
        Assert.Equal(ApiKeyAuthenticationOptions.Scheme, result.Principal.Identity?.AuthenticationType);
    }

    [Fact]
    public async Task ChaveValidaComTamanhoDiferente_RetornaFail()
    {
        // Garante que FixedTimeEquals lida com tamanhos diferentes sem throw e sem timing leak.
        var handler = await CreateHandlerAsync(configuredKey: ValidKey, header: ValidKey + "extra");
        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.NotNull(result.Failure);
    }

    // ─── helpers ────────────────────────────────────────────────────────────

    private static async Task<ApiKeyAuthenticationHandler> CreateHandlerAsync(
        string configuredKey,
        string? header)
    {
        return await CreateHandlerAsync(configuredKey, header is null ? null : new[] { header });
    }

    private static async Task<ApiKeyAuthenticationHandler> CreateHandlerAsync(
        string configuredKey,
        string[]? headerValues)
    {
        var options = new ApiKeyAuthenticationOptions { ApiKey = configuredKey };
        var optionsMonitor = new TestOptionsMonitor<ApiKeyAuthenticationOptions>(options);
        var handler = new ApiKeyAuthenticationHandler(optionsMonitor, NullLoggerFactory.Instance, UrlEncoder.Default);

        var httpContext = new DefaultHttpContext();
        if (headerValues is not null)
        {
            httpContext.Request.Headers[ApiKeyAuthenticationOptions.HeaderName] = headerValues;
        }

        var scheme = new AuthenticationScheme(
            ApiKeyAuthenticationOptions.Scheme,
            ApiKeyAuthenticationOptions.Scheme,
            typeof(ApiKeyAuthenticationHandler));
        await handler.InitializeAsync(scheme, httpContext);
        return handler;
    }

    private sealed class TestOptionsMonitor<T> : IOptionsMonitor<T>
    {
        private readonly T _value;
        public TestOptionsMonitor(T value) => _value = value;
        public T CurrentValue => _value;
        public T Get(string? name) => _value;
        public IDisposable OnChange(Action<T, string> listener) => new NoopDisposable();
        private sealed class NoopDisposable : IDisposable { public void Dispose() { } }
    }
}
