namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record AgentSummary(
    string Id,
    string Name,
    string Role,
    AgentStatus Status,
    string? CurrentTask
);
