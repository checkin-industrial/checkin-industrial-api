namespace AppTurismoIndustrial.Api.Features.Analytics;

public static class AnalyticsModule
{
    public static IServiceCollection AddAnalyticsFeature(this IServiceCollection services)
    {
        services.AddScoped<IMapaCalorIndustrialQuery, MapaCalorIndustrialQuery>();
        services.AddScoped<IHeatmapService, HeatmapService>();
        return services;
    }

    public static IEndpointRouteBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/analytics").WithTags("Analytics");
        group.MapGetHeatmap();
        return endpoints;
    }
}
