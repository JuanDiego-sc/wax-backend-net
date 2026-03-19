namespace Application.IntegrationEvents.SupportTicketEvents;

public class SupportTicketStatusChangedIntegrationEvent
{
    public required string TicketId { get; init; }
    public required string NewStatus { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
