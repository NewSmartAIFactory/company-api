namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/health").WithTags("Health");

        group.MapGet("/", () => Results.Ok(new
        {
            status = "healthy",
            timestampUtc = DateTimeOffset.UtcNow
        }));

        return app;
    }
}
