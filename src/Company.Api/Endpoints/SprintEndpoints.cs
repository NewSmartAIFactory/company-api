using System.Text.RegularExpressions;
using NewSmartAIFactory.CompanyApi.Models;
using NewSmartAIFactory.CompanyApi.Services;
using Npgsql;

namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static partial class SprintEndpoints
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase) { "Planned", "Active", "Completed", "Cancelled" };

    public static IEndpointRouteBuilder MapSprintEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sprints").WithTags("Sprints");
        group.MapGet("/", async (string? projectId, PostgresFactoryReadService reader, CancellationToken token) => Results.Ok(await reader.GetSprintsAsync(projectId, token)));
        group.MapGet("/{id}", async (string id, PostgresFactoryReadService reader, CancellationToken token) =>
        {
            var sprint = await reader.GetSprintAsync(id, token);
            return sprint is null ? Results.NotFound() : Results.Ok(sprint);
        });
        group.MapPost("/", async (SaveSprintRequest request, PostgresFactoryWriteService writer, PostgresFactoryReadService reader, CancellationToken token) =>
        {
            var error = Validate(request, true); if (error is not null) return error;
            try { var id = await writer.CreateSprintAsync(request, token); return Results.Created($"/api/sprints/{id}", await reader.GetSprintAsync(id, token)); }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation) { return Results.Conflict(new { error = "sprint_exists", message = $"Sprint '{request.Id}' already exists." }); }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation) { return Results.BadRequest(new { error = "project_not_found", message = $"Project '{request.ProjectId}' does not exist." }); }
        });
        group.MapPut("/{id}", async (string id, SaveSprintRequest request, PostgresFactoryWriteService writer, PostgresFactoryReadService reader, CancellationToken token) =>
        {
            var error = Validate(request, false); if (error is not null) return error;
            try { return await writer.UpdateSprintAsync(id, request, token) ? Results.Ok(await reader.GetSprintAsync(id, token)) : Results.NotFound(); }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation) { return Results.BadRequest(new { error = "project_not_found", message = $"Project '{request.ProjectId}' does not exist." }); }
        });
        group.MapDelete("/{id}", async (string id, string? actor, PostgresFactoryWriteService writer, CancellationToken token) =>
        {
            try { return await writer.DeleteSprintAsync(id, actor ?? "PM", token) ? Results.NoContent() : Results.NotFound(); }
            catch (InvalidOperationException ex) { return Results.Conflict(new { error = "sprint_in_use", message = ex.Message }); }
        });
        return app;
    }

    private static IResult? Validate(SaveSprintRequest request, bool requireId)
    {
        if (requireId && (string.IsNullOrWhiteSpace(request.Id) || !SprintIdRegex().IsMatch(request.Id))) return Results.BadRequest(new { error = "invalid_sprint_id", message = "Id must use lowercase letters, numbers, and hyphens." });
        if (string.IsNullOrWhiteSpace(request.ProjectId) || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Goal)) return Results.BadRequest(new { error = "required_fields", message = "ProjectId, name, and goal are required." });
        if (!AllowedStatuses.Contains(request.Status)) return Results.BadRequest(new { error = "invalid_status", message = $"Unsupported sprint status: {request.Status}." });
        if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate < request.StartDate) return Results.BadRequest(new { error = "invalid_dates", message = "EndDate cannot be before StartDate." });
        return null;
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SprintIdRegex();
}
