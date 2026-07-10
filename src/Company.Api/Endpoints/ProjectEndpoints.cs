using NewSmartAIFactory.CompanyApi.Services;

namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static class ProjectEndpoints
{
    public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects").WithTags("Projects");

        group.MapGet("/", async (PostgresFactoryReadService state, CancellationToken cancellationToken) =>
            Results.Ok(await state.GetProjectsAsync(cancellationToken)));

        return app;
    }
}