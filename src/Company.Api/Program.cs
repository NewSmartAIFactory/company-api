using System.Text.Json.Serialization;
using NewSmartAIFactory.CompanyApi.Endpoints;
using NewSmartAIFactory.CompanyApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton<FactoryStateService>();
builder.Services.AddScoped<PostgresFactoryReadService>();
builder.Services.AddScoped<PostgresFactoryWriteService>();
builder.Services.AddScoped<AgentRegistrySyncService>();
builder.Services.AddScoped<ApprovalWorkflowService>();
builder.Services.AddScoped<ReportGenerationService>();
builder.Services.AddScoped<EventStoreService>();
builder.Services.AddScoped<WorkflowRunnerService>();
builder.Services.AddScoped<AgentRunLogService>();
builder.Services.AddScoped<MemoryService>();
builder.Services.AddHttpClient("qdrant");
builder.Services.AddScoped<QdrantMemoryIndexService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Dashboard", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("Dashboard");

app.MapGet("/", () => Results.Ok(new
{
    name = "NewSmartAIFactory Company API",
    status = "running",
    version = "0.5.0",
    storage = "postgres",
    actions = new[] { "task-status", "decision-approval", "memory-semantic-search" }
}));

app.MapHealthEndpoints();
app.MapAgentEndpoints();
app.MapProjectEndpoints();
app.MapSprintEndpoints();
app.MapWorkflowEndpoints();
app.MapTaskEndpoints();
app.MapDecisionEndpoints();
app.MapReportEndpoints();
app.MapAuditEndpoints();
app.MapApprovalEndpoints();
app.MapEventEndpoints();
app.MapWorkflowRunEndpoints();
app.MapAgentRunEndpoints();
app.MapMemoryEndpoints();

using (var scope = app.Services.CreateScope())
{
    var registry = scope.ServiceProvider.GetRequiredService<AgentRegistrySyncService>();
    await registry.SyncAsync(CancellationToken.None);
}

app.Run();
