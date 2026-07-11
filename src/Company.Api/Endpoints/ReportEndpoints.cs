using NewSmartAIFactory.CompanyApi.Models;
using NewSmartAIFactory.CompanyApi.Services;
using Npgsql;

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

        group.MapPost("/", async (
            CreateReportRequest request,
            PostgresFactoryWriteService writer,
            PostgresFactoryReadService reader,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.ProjectId) ||
                string.IsNullOrWhiteSpace(request.ReportType) ||
                string.IsNullOrWhiteSpace(request.Period))
            {
                return Results.BadRequest(new { error = "invalid_report", message = "ProjectId, reportType, and period are required." });
            }

            if (request.ProgressPercent is < 0 or > 100)
            {
                return Results.BadRequest(new { error = "invalid_progress", message = "ProgressPercent must be between 0 and 100." });
            }

            try
            {
                var id = await writer.CreateReportAsync(request, cancellationToken);
                var reports = await reader.GetReportsAsync(cancellationToken);
                var report = reports.First(item => item.Id == id);
                return Results.Created($"/api/reports/{id}", report);
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                return Results.BadRequest(new { error = "project_not_found", message = $"Project '{request.ProjectId}' does not exist." });
            }
        });

        return app;
    }
}
