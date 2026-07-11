namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record SprintSummary(
    string Id,
    string ProjectId,
    string Name,
    string Goal,
    string Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    int TaskCount,
    int CompletedTaskCount
);
