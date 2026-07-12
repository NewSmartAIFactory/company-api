namespace NewSmartAIFactory.CompanyApi.Models;
public sealed record StartAgentRunRequest(string AgentId,string? TaskId,string? ProjectId,string InputText);
public sealed record CompleteAgentRunRequest(string Status,string? OutputText,long? DurationMs,IReadOnlyList<string>? FilesTouched,string? DecisionRequested,string? Error);
public sealed record AgentRunSummary(Guid Id,string AgentId,string? TaskId,string? ProjectId,string Status,string InputText,string? OutputText,long? DurationMs,IReadOnlyList<string> FilesTouched,string? DecisionRequested,string? Error,DateTimeOffset StartedAtUtc,DateTimeOffset? CompletedAtUtc);
