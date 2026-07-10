using NewSmartAIFactory.CompanyApi.Services;

namespace NewSmartAIFactory.CompanyApi.Endpoints;

public static class WorkflowEndpoints
{
    public static IEndpointRouteBuilder MapWorkflowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/workflows").WithTags("Workflows");

        group.MapGet("/", (FactoryStateService state) => Results.Ok(state.Workflows));

        return app;
    }
}
