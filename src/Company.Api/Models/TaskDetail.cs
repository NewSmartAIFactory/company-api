namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record TaskDetail(
    string Id, string ProjectId, string? SprintId, string Title, string? Description,
    string OwnerAgentId, string Status, string Priority, DateOnly? DueDate,
    IReadOnlyList<string> AcceptanceCriteria, IReadOnlyList<string> Dependencies,
    DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc,
    IReadOnlyList<AuditLogSummary> Activity
);
