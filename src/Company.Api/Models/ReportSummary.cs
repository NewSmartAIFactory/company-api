namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record ReportSummary(
    string Id,
    string ProjectId,
    string ReportType,
    string Period,
    int ProgressPercent,
    IReadOnlyList<string> Done,
    IReadOnlyList<string> Doing,
    IReadOnlyList<string> Blocked,
    IReadOnlyList<string> DecisionsNeeded
);