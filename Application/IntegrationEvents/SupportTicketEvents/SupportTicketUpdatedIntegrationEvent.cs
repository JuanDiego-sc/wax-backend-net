namespace Application.IntegrationEvents.SupportTicketEvents;

public class SupportTicketUpdatedIntegrationEvent
{
    public required string TicketId { get; init; }
    public required string Category { get; init; }
    public required string Status { get; init; }
    public required string Subject { get; init; }
    public required string Description { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
