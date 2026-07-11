namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record AgentDetail(
    string Id,
    string Name,
    string Role,
    string Department,
    AgentStatus Status,
    string? CurrentTask,
    string Prompt,
    string Rules,
    string Workflow,
    string MemoryScope,
    IReadOnlyList<string> Tools,
    string? DefinitionPath,
    DateTimeOffset? DefinitionUpdatedAtUtc
);
