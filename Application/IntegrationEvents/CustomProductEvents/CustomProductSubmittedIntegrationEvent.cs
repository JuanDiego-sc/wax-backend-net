namespace Application.IntegrationEvents.CustomProductEvents;

public class CustomProductSubmittedIntegrationEvent
{
    public required string CustomProductId { get; set; }
    public required string TaskId { get; set; }
    public required string OwnerUserId { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public long QuotedPrice { get; set; }
    public required string GlbUrl { get; set; }
    public required string DesignType { get; set; }
    public required string DesignMaterial { get; set; }
    public required string DesignColor { get; set; }
    public required string DesignShape { get; set; }
    public required string DesignDimensions { get; set; }
    public string? DesignDetails { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}