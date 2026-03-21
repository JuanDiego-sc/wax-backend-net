namespace Application.IntegrationEvents.SupportTicketEvents;

public class SupportTicketDeletedIntegrationEvent
{
    public required string TicketId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
