using NewSmartAIFactory.CompanyApi.Services;

namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static class TaskEndpoints
{
    public static IEndpointRouteBuilder MapTaskEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tasks").WithTags("Tasks");

        group.MapGet("/", (FactoryStateService state) => Results.Ok(state.Tasks));

        group.MapGet("/{id}", (string id, FactoryStateService state) =>
        {
            var task = state.Tasks.FirstOrDefault(item => item.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
            return task is null ? Results.NotFound() : Results.Ok(task);
        });

        return app;
    }
}