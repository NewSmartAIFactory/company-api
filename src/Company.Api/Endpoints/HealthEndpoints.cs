using NewSmartAIFactory.CompanyApi.Services;

namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/health").WithTags("Health");

        group.MapGet("/", async (QdrantMemoryIndexService qdrant, CancellationToken cancellationToken) =>
        {
            var qdrantHealthy = false;
            try { qdrantHealthy = await qdrant.IsHealthyAsync(cancellationToken); }
            catch (HttpRequestException) { }
            var status = qdrantHealthy ? "healthy" : "degraded";
            return Results.Ok(new
            {
                status,
                timestampUtc = DateTimeOffset.UtcNow,
                components = new { qdrant = qdrantHealthy }
            });
        });

        return app;
    }
}
