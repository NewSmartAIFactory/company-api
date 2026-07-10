namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record ProjectSummary(
    string Id,
    string Name,
    string Status,
    int ProgressPercent,
    string? CurrentSprint
);
