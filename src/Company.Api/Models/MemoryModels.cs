namespace NewSmartAIFactory.CompanyApi.Models;
public sealed record CreateMemoryRequest(string Scope,string MemoryType,string Title,string Content,string? ProjectId,string? AgentId,string? Source);
public sealed record MemorySummary(Guid Id,string Scope,string MemoryType,string Title,string Content,string? ProjectId,string? AgentId,string? Source,bool IsObsolete,DateTimeOffset CreatedAtUtc,DateTimeOffset UpdatedAtUtc);
