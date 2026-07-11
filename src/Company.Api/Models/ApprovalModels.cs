namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record ApprovalSummary(string Id, string ProjectId, string Title, string RequestedBy, string Status, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc);
public sealed record ApprovalHistoryItem(long Id, string Action, string Actor, string? Comment, DateTimeOffset CreatedAtUtc);
public sealed record ApprovalDetail(string Id, string ProjectId, string Title, string Description, string RequestedBy, string Status,
    string? ScopeImpact, string? CostImpact, string? TimelineImpact, string? SecurityImpact, string? ArchitectureImpact,
    DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc, IReadOnlyList<ApprovalHistoryItem> History);
public sealed record CreateApprovalRequest(string ProjectId, string Title, string Description, string RequestedBy,
    string? ScopeImpact, string? CostImpact, string? TimelineImpact, string? SecurityImpact, string? ArchitectureImpact);
public sealed record ApprovalActionRequest(string Action, string Actor, string? Comment);
