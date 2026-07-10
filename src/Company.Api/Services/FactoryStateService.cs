using NewSmartAIFactory.CompanyApi.Models;

namespace NewSmartAIFactory.CompanyApi.Services;

public sealed class FactoryStateService
{
    public IReadOnlyList<AgentSummary> Agents { get; } =
    [
        new("ceo-assistant", "CEO Assistant", "Executive", AgentStatus.Idle, null),
        new("pm", "Project Manager", "Management", AgentStatus.Idle, null),
        new("ba", "Business Analyst", "Product", AgentStatus.Idle, null),
        new("architect", "Solution Architect", "Engineering", AgentStatus.Idle, null),
        new("backend", "Backend Agent", "Engineering", AgentStatus.Idle, null),
        new("frontend", "Frontend Agent", "Engineering", AgentStatus.Idle, null),
        new("qc", "QC Agent", "Quality", AgentStatus.Idle, null),
        new("devops", "DevOps Agent", "Operations", AgentStatus.Idle, null)
    ];

    public IReadOnlyList<ProjectSummary> Projects { get; } =
    [
        new("company-os", "AI Company OS", "Foundation", 10, "Sprint 1.1")
    ];

    public IReadOnlyList<WorkflowSummary> Workflows { get; } =
    [
        new(
            "software-delivery",
            "Software Delivery",
            "Draft",
            ["CEO Request", "PM Planning", "BA Analysis", "Architecture", "Development", "QC", "Deployment", "Report"]
        )
    ];
}
