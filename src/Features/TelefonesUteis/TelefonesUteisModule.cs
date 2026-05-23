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
        group.MapListTelefonesUteis();
        group.MapGetTelefoneUtilById();
        group.MapCreateTelefoneUtil();
        group.MapUpdateTelefoneUtil();
        group.MapDeleteTelefoneUtil();
        return endpoints;
    }
}
