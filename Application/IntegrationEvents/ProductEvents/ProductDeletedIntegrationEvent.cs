namespace Application.IntegrationEvents.ProductEvents;

public class ProductDeletedIntegrationEvent
{
    public required string ProductId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}