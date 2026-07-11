namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record AuditLogSummary(
    long Id,
    string Action,
    string EntityType,
    string EntityId,
    string Actor,
    string? PreviousValue,
    string? NewValue,
    string? Reason,
    DateTimeOffset CreatedAtUtc
);
