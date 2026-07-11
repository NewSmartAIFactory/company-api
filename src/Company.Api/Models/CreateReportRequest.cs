namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record CreateReportRequest(
    string ProjectId,
    string ReportType,
    string Period,
    int ProgressPercent,
    IReadOnlyList<string>? Done,
    IReadOnlyList<string>? Doing,
    IReadOnlyList<string>? Blocked,
    IReadOnlyList<string>? DecisionsNeeded,
    string? PublishedBy
);
