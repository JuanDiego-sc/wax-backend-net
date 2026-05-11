namespace Domain.ProductAggregate;

public class CustomProductDesign
{
    public required string Type { get; set; }
    public required string Material { get; set; }
    public required string Color { get; set; }
    public required string Shape { get; set; }
    public required string Dimensions { get; set; }
    public string? Details { get; set; }
}