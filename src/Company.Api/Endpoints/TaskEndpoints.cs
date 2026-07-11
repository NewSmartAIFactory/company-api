using NewSmartAIFactory.CompanyApi.Models;
using NewSmartAIFactory.CompanyApi.Services;

namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks");

        group.MapGet("/", async (PostgresFactoryReadService state, CancellationToken cancellationToken) =>
            Results.Ok(await state.GetTasksAsync(cancellationToken)));

        group.MapGet("/{id}", async (string id, PostgresFactoryReadService state, CancellationToken cancellationToken) =>
        {
            var tasks = await state.GetTasksAsync(cancellationToken);
            var task = tasks.FirstOrDefault(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return task is null ? Results.NotFound() : Results.Ok(task);
        });

        group.MapPatch("/{id}/status", async (
            string id,
            UpdateTaskStatusRequest request,
            PostgresFactoryWriteService writer,
            PostgresFactoryReadService reader,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return Results.BadRequest(new { error = "status_required", message = "Status is required." });
            }

            try
            {
                var updated = await writer.UpdateTaskStatusAsync(id, request.Status.Trim(), "CEO", null, cancellationToken);
                if (!updated)
                {
                    return Results.NotFound();
                }

                var tasks = await reader.GetTasksAsync(cancellationToken);
                var task = tasks.First(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
                return Results.Ok(task);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = "invalid_status", message = ex.Message });
            }
        });

        return app;
    }
}
