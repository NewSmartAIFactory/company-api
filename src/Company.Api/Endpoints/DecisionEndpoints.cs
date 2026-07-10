using NewSmartAIFactory.CompanyApi.Services;

namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static class DecisionEndpoints
{
    public static IEndpointRouteBuilder MapDecisionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/decisions").WithTags("Decisions");

        group.MapGet("/", (FactoryStateService state) => Results.Ok(state.Decisions));

        group.MapGet("/{id}", (string id, FactoryStateService state) =>
        {
            var decision = state.Decisions.FirstOrDefault(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return decision is null ? Results.NotFound() : Results.Ok(decision);
        });

        return app;
    }
}