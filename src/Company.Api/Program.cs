using System.Text.Json.Serialization;
using NewSmartAIFactory.CompanyApi.Endpoints;
using NewSmartAIFactory.CompanyApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton<FactoryStateService>();
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
    version = "0.2.1"
}));

app.MapHealthEndpoints();
app.MapAgentEndpoints();
app.MapProjectEndpoints();
app.MapWorkflowEndpoints();
app.MapTaskEndpoints();
app.MapDecisionEndpoints();
app.MapReportEndpoints();

app.Run();