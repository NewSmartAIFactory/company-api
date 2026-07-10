namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record WorkflowSummary(
    string Id,
    string Name,
    string Status,
    IReadOnlyList<string> Steps
);
