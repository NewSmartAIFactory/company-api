namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record SaveTaskRequest(
    string? Id, string ProjectId, string? SprintId, string Title, string? Description,
    string OwnerAgentId, string Status, string Priority, DateOnly? DueDate,
    IReadOnlyList<string>? AcceptanceCriteria, IReadOnlyList<string>? Dependencies, string? Actor
);
