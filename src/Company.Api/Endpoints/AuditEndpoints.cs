using NewSmartAIFactory.CompanyApi.Services;

namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/audit-logs").WithTags("Audit");

        group.MapGet("/", async (int? limit, PostgresFactoryReadService state, CancellationToken cancellationToken) =>
            Results.Ok(await state.GetAuditLogsAsync(Math.Clamp(limit ?? 50, 1, 200), cancellationToken)));

        return app;
    }
}
