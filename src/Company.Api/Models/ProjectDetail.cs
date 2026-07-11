namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record ProjectDetail(
    string Id,
    string Name,
    string Status,
    int ProgressPercent,
    string? CurrentSprint,
    string? Description,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc
);
