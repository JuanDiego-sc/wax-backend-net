namespace Application.IntegrationEvents.OrderEvents;

public class OrderStatusChangedIntegrationEvent
{
    public required string OrderId { get; init; }
    public required string NewStatus { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}