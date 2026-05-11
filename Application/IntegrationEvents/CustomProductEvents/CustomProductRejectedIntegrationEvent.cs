namespace Application.IntegrationEvents.CustomProductEvents;

public class CustomProductRejectedIntegrationEvent
{
    public required string CustomProductId { get; set; }
    public required string OwnerUserId { get; set; }
    public required string Reason { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}