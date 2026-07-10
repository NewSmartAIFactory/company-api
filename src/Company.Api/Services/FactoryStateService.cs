using NewSmartAIFactory.CompanyApi.Models;

namespace NewSmartAIFactory.CompanyApi.Services;

public sealed class FactoryStateService
{
    public IReadOnlyList<AgentSummary> Agents { get; } =
    [
        new("ceo-assistant", "CEO Assistant", "Executive", AgentStatus.Idle, null),
        new("pm", "Project Manager", "Management", AgentStatus.Working, "Sprint 1.2 planning"),
        new("ba", "Business Analyst", "Product", AgentStatus.Idle, null),
        new("architect", "Solution Architect", "Engineering", AgentStatus.Idle, null),
        new("backend", "Backend Agent", "Engineering", AgentStatus.Working, "Company API endpoints"),
        new("frontend", "Frontend Agent", "Engineering", AgentStatus.Idle, null),
        new("qc", "QC Agent", "Quality", AgentStatus.Idle, null),
        new("devops", "DevOps Agent", "Operations", AgentStatus.Idle, null),
        new("security", "Security Reviewer", "Quality", AgentStatus.Idle, null),
        new("support", "Support Agent", "Customer Success", AgentStatus.Idle, null)
    ];

    public IReadOnlyList<ProjectSummary> Projects { get; } =
    [
        new("company-os", "AI Company OS", "Foundation", 22, "Sprint 1.2")
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

    public IReadOnlyList<TaskSummary> Tasks { get; } =
    [
        new(
            "TASK-001",
            "company-os",
            "Create local infrastructure runtime",
            "devops",
            "Done",
            "High",
            ["Docker Compose starts PostgreSQL, Redis, Qdrant, and RabbitMQ"]
        ),
        new(
            "TASK-002",
            "company-os",
            "Create Company API skeleton",
            "backend",
            "Done",
            "High",
            ["API exposes health, agents, projects, and workflows"]
        ),
        new(
            "TASK-003",
            "company-os",
            "Create Company Dashboard skeleton",
            "frontend",
            "Done",
            "High",
            ["Dashboard reads API and displays operating status"]
        ),
        new(
            "TASK-004",
            "company-os",
            "Add tasks, decisions, and reports endpoints",
            "backend",
            "Doing",
            "High",
            ["API exposes tasks, decisions, and reports"]
        )
    ];

    public IReadOnlyList<DecisionSummary> Decisions { get; } =
    [
        new(
            "DEC-001",
            "company-os",
            "Use modular monolith before microservices",
            "Approved",
            "architect",
            "Architecture",
            "Start simple with module boundaries and split later only when needed"
        ),
        new(
            "DEC-002",
            "company-os",
            "Use .NET 10 on local machine",
            "Approved",
            "backend",
            "Runtime",
            "Target net10.0 because installed SDK/runtime is .NET 10"
        )
    ];

    public IReadOnlyList<ReportSummary> Reports { get; } =
    [
        new(
            "RPT-001",
            "company-os",
            "Sprint",
            "Sprint 1.1",
            22,
            [
                "GitHub repositories connected",
                "Infrastructure is running",
                "Company API is running",
                "Dashboard is running",
                "Agent definitions, knowledge, templates, and OS docs bootstrapped"
            ],
            [
                "Expanding API into tasks, decisions, and reports"
            ],
            [],
            [
                "Confirm next priority: persistence or Telegram integration"
            ]
        )
    ];
}