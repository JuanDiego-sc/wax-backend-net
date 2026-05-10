namespace Application.IntegrationEvents.CustomProductEvents;

public class CustomProductPriceUpdatedIntegrationEvent
{
    public required string CustomProductId { get; set; }
    public required string OwnerUserId { get; set; }
    public required string Status { get; set; }
    public long Price { get; set; }
    public required string ProposalId { get; set; }
    public required string ProposalSource { get; set; }
    public string? Comment { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}