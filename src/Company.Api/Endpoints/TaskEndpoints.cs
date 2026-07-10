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

        return app;
    }
}