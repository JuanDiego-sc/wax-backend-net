namespace Application.IntegrationEvents.ProductEvents;

public class ProductStockChangedIntegrationEvent
{
    public required string ProductId { get; set; }
    public int NewQuantity { get; set; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;

}