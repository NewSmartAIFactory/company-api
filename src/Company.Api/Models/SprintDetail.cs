namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record SprintDetail(
    string Id,
    string ProjectId,
    string Name,
    string Goal,
    string Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<TaskSummary> Backlog
);
