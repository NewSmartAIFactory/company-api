namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record SaveProjectRequest(
    string? Id,
    string Name,
    string Status,
    int ProgressPercent,
    string? CurrentSprint,
    string? Description,
    string? Actor
);
