using NewSmartAIFactory.CompanyApi.Services;

namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static class AgentEndpoints
{
    public static IEndpointRouteBuilder MapAgentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/agents").WithTags("Agents");

        group.MapGet("/", async (PostgresFactoryReadService state, CancellationToken cancellationToken) =>
            Results.Ok(await state.GetAgentsAsync(cancellationToken)));

        group.MapGet("/{id}", async (string id, PostgresFactoryReadService state, CancellationToken cancellationToken) =>
        {
            var agent = await state.GetAgentAsync(id, cancellationToken);
            return agent is null ? Results.NotFound() : Results.Ok(agent);
        });

        group.MapPost("/sync", async (AgentRegistrySyncService registry, CancellationToken cancellationToken) =>
        {
            var count = await registry.SyncAsync(cancellationToken);
            return Results.Ok(new { synced = count });
        });

        return app;
    }
}
