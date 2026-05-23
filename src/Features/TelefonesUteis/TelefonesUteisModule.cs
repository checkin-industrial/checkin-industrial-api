namespace AppTurismoIndustrial.Api.Features.TelefonesUteis;

public static class TelefonesUteisModule
{
    public static IServiceCollection AddTelefonesUteisFeature(this IServiceCollection services)
    {
        services.AddScoped<ITelefoneUtilService, TelefoneUtilService>();
        services.AddScoped<ITelefoneUtilQuery, TelefoneUtilQuery>();
        return services;
    }

    public static IEndpointRouteBuilder MapTelefonesUteisEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/telefones-uteis").WithTags("TelefonesUteis");

        // Reads - publicos, com output cache
        group.MapListTelefonesUteis().CacheOutput("ReadEndpoint");
        group.MapGetTelefoneUtilById().CacheOutput("ReadEndpoint");

        // Writes - protegidos por API Key
        group.MapCreateTelefoneUtil().RequireAuthorization();
        group.MapUpdateTelefoneUtil().RequireAuthorization();
        group.MapDeleteTelefoneUtil().RequireAuthorization();

        return endpoints;
    }
}
