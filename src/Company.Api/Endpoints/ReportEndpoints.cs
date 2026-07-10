using NewSmartAIFactory.CompanyApi.Services;

namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports").WithTags("Reports");

        group.MapGet("/", async (PostgresFactoryReadService state, CancellationToken cancellationToken) =>
            Results.Ok(await state.GetReportsAsync(cancellationToken)));

        group.MapGet("/{id}", async (string id, PostgresFactoryReadService state, CancellationToken cancellationToken) =>
        {
            var reports = await state.GetReportsAsync(cancellationToken);
            var report = reports.FirstOrDefault(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return report is null ? Results.NotFound() : Results.Ok(report);
        });

        return app;
    }
}