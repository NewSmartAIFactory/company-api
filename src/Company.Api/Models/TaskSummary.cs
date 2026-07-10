namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record TaskSummary(
    string Id,
    string ProjectId,
    string Title,
    string OwnerAgentId,
    string Status,
    string Priority,
    IReadOnlyList<string> AcceptanceCriteria
);