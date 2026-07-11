using System.Text.Json;
namespace NewSmartAIFactory.CompanyApi.Models;
public sealed record DomainEventEnvelope(Guid Id,string EventType,string AggregateType,string AggregateId,string? ProjectId,string CorrelationId,string Actor,JsonElement Payload,DateTimeOffset OccurredAtUtc,int PublishAttempts=0,DateTimeOffset? PublishedAtUtc=null,string? LastPublishError=null);
public sealed record CreateDomainEventRequest(string EventType,string AggregateType,string AggregateId,string? ProjectId,string? CorrelationId,string? Actor,JsonElement? Payload);
