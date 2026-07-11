namespace NewSmartAIFactory.CompanyApi.Models;
public sealed record StartWorkflowRequest(string WorkflowId,string ProjectId,string InitiatedBy,string? Context);
public sealed record WorkflowRunSummary(Guid Id,string WorkflowId,string ProjectId,string Status,int CurrentStepOrder,string CurrentStep,string? NextAgent,string InitiatedBy,DateTimeOffset StartedAtUtc,DateTimeOffset UpdatedAtUtc,DateTimeOffset? CompletedAtUtc);
public sealed record AdvanceWorkflowRequest(string Actor,string? Note);
