using System.Text.Json;
namespace NewSmartAIFactory.CompanyApi.Models;
public sealed record DomainEventEnvelope(Guid Id,string EventType,string AggregateType,string AggregateId,string? ProjectId,string CorrelationId,string Actor,JsonElement Payload,DateTimeOffset OccurredAtUtc);
public sealed record CreateDomainEventRequest(string EventType,string AggregateType,string AggregateId,string? ProjectId,string? CorrelationId,string? Actor,JsonElement? Payload);
