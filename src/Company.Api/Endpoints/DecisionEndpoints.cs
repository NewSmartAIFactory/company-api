using NewSmartAIFactory.CompanyApi.Models;
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

        group.MapPost("/{id}/approve", async (
            string id,
            DecisionActionRequest request,
            PostgresFactoryWriteService writer,
            PostgresFactoryReadService reader,
            CancellationToken cancellationToken) =>
        {
            var updated = await writer.ApproveDecisionAsync(id, request.DecidedBy ?? "CEO", request.Reason, cancellationToken);
            if (!updated)
            {
                return Results.NotFound();
            }

            var decisions = await reader.GetDecisionsAsync(cancellationToken);
            var decision = decisions.First(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Results.Ok(decision);
        });

        group.MapPost("/{id}/reject", async (
            string id,
            DecisionActionRequest request,
            PostgresFactoryWriteService writer,
            PostgresFactoryReadService reader,
            CancellationToken cancellationToken) =>
        {
            var updated = await writer.RejectDecisionAsync(id, request.DecidedBy ?? "CEO", request.Reason, cancellationToken);
            if (!updated)
            {
                return Results.NotFound();
            }

            var decisions = await reader.GetDecisionsAsync(cancellationToken);
            var decision = decisions.First(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return Results.Ok(decision);
        });

        return app;
    }
}
