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
            var postgresHealthy = await CanConnectAsync("localhost", 5432, cancellationToken);
            var redisHealthy = await CanConnectAsync("localhost", 6379, cancellationToken);
            var rabbitHealthy = await CanConnectAsync("localhost", 5672, cancellationToken);
            var status = qdrantHealthy && postgresHealthy && redisHealthy && rabbitHealthy ? "healthy" : "degraded";
            return Results.Ok(new
            {
                status,
                timestampUtc = DateTimeOffset.UtcNow,
                components = new { qdrant = qdrantHealthy, postgres = postgresHealthy, redis = redisHealthy, rabbitmq = rabbitHealthy }
            });
        });

        return app;
    }

    private static async Task<bool> CanConnectAsync(string host, int port, CancellationToken cancellationToken)
    {
        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromMilliseconds(500));
            await client.ConnectAsync(host, port, timeout.Token);
            return client.Connected;
        }
        catch { return false; }
    }
}
