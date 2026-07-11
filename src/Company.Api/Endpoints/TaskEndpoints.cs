using System.Text.RegularExpressions;
using NewSmartAIFactory.CompanyApi.Models;
using NewSmartAIFactory.CompanyApi.Services;
using Npgsql;
using System.Text.Json;

namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static partial class TaskEndpoints
{
    private static readonly HashSet<string> Statuses = new(StringComparer.OrdinalIgnoreCase) { "Todo", "Doing", "Done", "Blocked" };
    private static readonly HashSet<string> Priorities = new(StringComparer.OrdinalIgnoreCase) { "Low", "Medium", "High", "Critical" };
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks");
        group.MapGet("/", async (PostgresFactoryReadService reader, CancellationToken token) => Results.Ok(await reader.GetTasksAsync(token)));
        group.MapGet("/{id}", async (string id, PostgresFactoryReadService reader, CancellationToken token) => { var task = await reader.GetTaskAsync(id, token); return task is null ? Results.NotFound() : Results.Ok(task); });
        group.MapPost("/", async (SaveTaskRequest request, PostgresFactoryWriteService writer, PostgresFactoryReadService reader, EventStoreService events, CancellationToken token) =>
        {
            var error = Validate(request, true); if (error is not null) return error;
            try { var id = await writer.CreateTaskAsync(request, token); await events.AppendAsync(new CreateDomainEventRequest("TaskAssigned","Task",id,request.ProjectId,null,request.Actor,JsonSerializer.SerializeToElement(new { request.OwnerAgentId, request.SprintId, request.Priority })),token); return Results.Created($"/api/tasks/{id}", await reader.GetTaskAsync(id, token)); }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation) { return Results.Conflict(new { error = "task_exists", message = $"Task '{request.Id}' already exists." }); }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation) { return Results.BadRequest(new { error = "invalid_reference", message = "Project, sprint, agent, or dependency does not exist." }); }
        });
        group.MapPut("/{id}", async (string id, SaveTaskRequest request, PostgresFactoryWriteService writer, PostgresFactoryReadService reader, CancellationToken token) =>
        {
            var error = Validate(request, false); if (error is not null) return error;
            try { return await writer.UpdateTaskAsync(id, request, token) ? Results.Ok(await reader.GetTaskAsync(id, token)) : Results.NotFound(); }
            catch (PostgresException ex) when (ex.SqlState is PostgresErrorCodes.ForeignKeyViolation or PostgresErrorCodes.CheckViolation) { return Results.BadRequest(new { error = "invalid_reference", message = "Project, sprint, agent, or dependency is invalid." }); }
        });
        group.MapDelete("/{id}", async (string id, string? actor, PostgresFactoryWriteService writer, CancellationToken token) => await writer.DeleteTaskAsync(id, actor ?? "PM", token) ? Results.NoContent() : Results.NotFound());
        group.MapPatch("/{id}/status", async (string id, UpdateTaskStatusRequest request, PostgresFactoryWriteService writer, PostgresFactoryReadService reader, EventStoreService events, CancellationToken token) =>
        {
            if (string.IsNullOrWhiteSpace(request.Status)) return Results.BadRequest(new { error = "status_required", message = "Status is required." });
            try { if(!await writer.UpdateTaskStatusAsync(id, request.Status.Trim(), "CEO", null, token))return Results.NotFound();var task=(await reader.GetTasksAsync(token)).First(x=>x.Id.Equals(id,StringComparison.OrdinalIgnoreCase));var type=request.Status.Equals("Done",StringComparison.OrdinalIgnoreCase)?"WorkCompleted":request.Status.Equals("Blocked",StringComparison.OrdinalIgnoreCase)?"AgentBlocked":"TaskStatusChanged";await events.AppendAsync(new CreateDomainEventRequest(type,"Task",id,task.ProjectId,null,"CEO",JsonSerializer.SerializeToElement(new{status=request.Status})),token);return Results.Ok(task); }
            catch (ArgumentException ex) { return Results.BadRequest(new { error = "invalid_status", message = ex.Message }); }
        });
        return app;
    }
    private static IResult? Validate(SaveTaskRequest request, bool requireId)
    {
        if (requireId && (string.IsNullOrWhiteSpace(request.Id) || !IdRegex().IsMatch(request.Id))) return Results.BadRequest(new { error = "invalid_task_id", message = "Id must use letters, numbers, and hyphens." });
        if (string.IsNullOrWhiteSpace(request.ProjectId) || string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.OwnerAgentId)) return Results.BadRequest(new { error = "required_fields", message = "ProjectId, title, and ownerAgentId are required." });
        if (!Statuses.Contains(request.Status) || !Priorities.Contains(request.Priority)) return Results.BadRequest(new { error = "invalid_state", message = "Status or priority is invalid." });
        if (request.Dependencies?.Any(x => x.Equals(request.Id, StringComparison.OrdinalIgnoreCase)) == true) return Results.BadRequest(new { error = "self_dependency", message = "A task cannot depend on itself." });
        return null;
    }
    [GeneratedRegex("^[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*$")]
    private static partial Regex IdRegex();
}
