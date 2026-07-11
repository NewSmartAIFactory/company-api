using System.Text.RegularExpressions;
using NewSmartAIFactory.CompanyApi.Models;
using NewSmartAIFactory.CompanyApi.Services;
using Npgsql;

namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static partial class ProjectEndpoints
{
    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draft", "Active", "Paused", "Completed", "Archived", "Foundation"
    };

    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Projects");

        group.MapGet("/", async (PostgresFactoryReadService state, CancellationToken cancellationToken) =>
            Results.Ok(await state.GetProjectsAsync(cancellationToken)));

        group.MapGet("/{id}", async (string id, PostgresFactoryReadService state, CancellationToken cancellationToken) =>
        {
            var project = await state.GetProjectAsync(id, cancellationToken);
            return project is null ? Results.NotFound() : Results.Ok(project);
        });

        group.MapPost("/", async (SaveProjectRequest request, PostgresFactoryWriteService writer, PostgresFactoryReadService reader, CancellationToken cancellationToken) =>
        {
            var validation = Validate(request, requireId: true);
            if (validation is not null) return validation;
            try
            {
                var id = await writer.CreateProjectAsync(request, cancellationToken);
                return Results.Created($"/api/projects/{id}", await reader.GetProjectAsync(id, cancellationToken));
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Results.Conflict(new { error = "project_exists", message = $"Project '{request.Id}' already exists." });
            }
        });

        group.MapPut("/{id}", async (string id, SaveProjectRequest request, PostgresFactoryWriteService writer, PostgresFactoryReadService reader, CancellationToken cancellationToken) =>
        {
            var validation = Validate(request, requireId: false);
            if (validation is not null) return validation;
            var updated = await writer.UpdateProjectAsync(id, request, cancellationToken);
            return updated ? Results.Ok(await reader.GetProjectAsync(id, cancellationToken)) : Results.NotFound();
        });

        group.MapDelete("/{id}", async (string id, string? actor, PostgresFactoryWriteService writer, CancellationToken cancellationToken) =>
        {
            try
            {
                return await writer.DeleteProjectAsync(id, actor ?? "CEO", cancellationToken) ? Results.NoContent() : Results.NotFound();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
            {
                return Results.Conflict(new { error = "project_in_use", message = "Archive projects with related work instead of deleting them." });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = "project_in_use", message = ex.Message });
            }
        });

        return app;
    }

    private static IResult? Validate(SaveProjectRequest request, bool requireId)
    {
        if (requireId && (string.IsNullOrWhiteSpace(request.Id) || !ProjectIdRegex().IsMatch(request.Id)))
            return Results.BadRequest(new { error = "invalid_project_id", message = "Id must use lowercase letters, numbers, and hyphens." });
        if (string.IsNullOrWhiteSpace(request.Name))
            return Results.BadRequest(new { error = "name_required", message = "Name is required." });
        if (!AllowedStatuses.Contains(request.Status))
            return Results.BadRequest(new { error = "invalid_status", message = $"Unsupported project status: {request.Status}." });
        if (request.ProgressPercent is < 0 or > 100)
            return Results.BadRequest(new { error = "invalid_progress", message = "ProgressPercent must be between 0 and 100." });
        return null;
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex ProjectIdRegex();
}
