namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record SaveSprintRequest(
    string? Id,
    string ProjectId,
    string Name,
    string Goal,
    string Status,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Actor
);
