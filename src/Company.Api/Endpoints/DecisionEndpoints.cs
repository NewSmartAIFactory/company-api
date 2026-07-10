using NewSmartAIFactory.CompanyApi.Services;

namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static class DecisionEndpoints
{
    public static IEndpointRouteBuilder MapDecisionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/decisions").WithTags("Decisions");

        group.MapGet("/", async (PostgresFactoryReadService state, CancellationToken cancellationToken) =>
            Results.Ok(await state.GetDecisionsAsync(cancellationToken)));

        group.MapGet("/{id}", async (string id, PostgresFactoryReadService state, CancellationToken cancellationToken) =>
        {
            var decisions = await state.GetDecisionsAsync(cancellationToken);
            var decision = decisions.FirstOrDefault(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return decision is null ? Results.NotFound() : Results.Ok(decision);
        });

        return app;
    }
}