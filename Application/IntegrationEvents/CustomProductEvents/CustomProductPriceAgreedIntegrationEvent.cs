namespace Application.IntegrationEvents.CustomProductEvents;

public class CustomProductPriceAgreedIntegrationEvent
{
    public required string CustomProductId { get; set; }
    public required string OwnerUserId { get; set; }
    public required string BasketId { get; set; }
    public long AgreedPrice { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}