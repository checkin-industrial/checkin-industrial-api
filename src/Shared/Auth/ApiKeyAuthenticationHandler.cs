using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace AppTurismoIndustrial.Api.Shared.Auth;

/// <summary>
/// Autenticacao via header `X-Api-Key`. Usada para proteger endpoints de escrita
/// (Create/Update/Delete/Import/Upload). Reads do widget continuam anonimos.
///
/// O valor esperado vem de `Auth:ApiKey` em appsettings (ou env var `Auth__ApiKey`).
/// Em prod, esse valor mora num secret do Railway/GitHub Actions, nunca em config commitada.
/// </summary>
public sealed class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    public const string Scheme = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    public string ApiKey { get; set; } = string.Empty;
}

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Se a config nao tem ApiKey definida, o handler deixa o pipeline anonimo
        // (Authorization vai decidir 401 se a rota exigir). Isso evita "lockout"
        // acidental em dev quando esquece de setar a env.
        if (string.IsNullOrWhiteSpace(Options.ApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var provided)
            || string.IsNullOrWhiteSpace(provided))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!CryptographicEquals(provided!, Options.ApiKey))
        {
            return Task.FromResult(AuthenticateResult.Fail("API Key invalida."));
        }

        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.Name, "api-key-client") },
            ApiKeyAuthenticationOptions.Scheme);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), ApiKeyAuthenticationOptions.Scheme);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    /// <summary>Comparacao em tempo constante para evitar timing attacks.</summary>
    private static bool CryptographicEquals(string a, string b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }
        var diff = 0;
        for (var i = 0; i < a.Length; i++)
        {
            diff |= a[i] ^ b[i];
        }
        return diff == 0;
    }
}
