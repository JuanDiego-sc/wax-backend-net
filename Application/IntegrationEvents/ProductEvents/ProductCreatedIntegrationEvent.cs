namespace Application.IntegrationEvents.ProductEvents;

public class ProductCreatedIntegrationEvent
{
    public required string ProductId { get; set; }
    public required string Name { get; set; } 
    public required string Description { get; set; }
    public long Price { get; set; }
    public required string PictureUrl { get; set; }
    public required string  Type { get; set; }
    public required string Brand { get; set; }
    public int QuantityInStock { get; set; }
    public string? PublicId { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}