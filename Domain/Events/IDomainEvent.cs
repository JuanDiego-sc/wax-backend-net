namespace Domain.Events;

public interface IDomainEvent
{
    string EventId { get; }
    DateTime OccurredAt { get; }
}