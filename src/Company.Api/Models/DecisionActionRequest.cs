namespace NewSmartAIFactory.CompanyApi.Models;

public sealed record DecisionActionRequest(
    string? DecidedBy,
    string? Reason
);