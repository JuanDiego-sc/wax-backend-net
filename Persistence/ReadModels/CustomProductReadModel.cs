namespace Persistence.ReadModels;

public class CustomProductReadModel
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public long Price { get; set; }
    public required string PictureUrl { get; set; }
    public required string TaskId { get; set; }
    public required string GlbUrl { get; set; }
    public required string OwnerUserId { get; set; }
    public required string Status { get; set; }
    public required string DesignType { get; set; }
    public required string DesignMaterial { get; set; }
    public required string DesignColor { get; set; }
    public required string DesignShape { get; set; }
    public required string DesignDimensions { get; set; }
    public string? DesignDetails { get; set; }
    public long? AgreedPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}