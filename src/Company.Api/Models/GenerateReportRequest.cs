namespace NewSmartAIFactory.CompanyApi.Models;
public sealed record GenerateReportRequest(string ProjectId, string ReportType, string Period, string? PublishedBy);
