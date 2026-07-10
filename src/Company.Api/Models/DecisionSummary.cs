namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record DecisionSummary(
    string Id,
    string ProjectId,
    string Title,
    string Status,
    string RequestedBy,
    string Impact,
    string Recommendation
);