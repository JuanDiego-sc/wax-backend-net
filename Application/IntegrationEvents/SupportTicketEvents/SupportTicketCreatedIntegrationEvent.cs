namespace Application.IntegrationEvents.SupportTicketEvents;

public class SupportTicketCreatedIntegrationEvent
{
    public required string TicketId { get; init; }
    public required string UserId { get; init; }
    public required string UserEmail { get; init; }
    public required string UserFullName { get; init; }
    public required string OrderId { get; init; }
    public required string Category { get; init; }
    public required string Status { get; init; }
    public required string Subject { get; init; }
    public required string Description { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
