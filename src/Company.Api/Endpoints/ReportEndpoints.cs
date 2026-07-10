using NewSmartAIFactory.CompanyApi.Services;

namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports").WithTags("Reports");

        group.MapGet("/", (FactoryStateService state) => Results.Ok(state.Reports));

        group.MapGet("/{id}", (string id, FactoryStateService state) =>
        {
            var report = state.Reports.FirstOrDefault(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return report is null ? Results.NotFound() : Results.Ok(report);
        });

        return app;
    }
}