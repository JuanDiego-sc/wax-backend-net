using Domain.ProductAggregate;

namespace Application.CustomProducts.DTOs;

public record CustomProductDesignRequest{
    public string Type { get; init; } = string.Empty;
    public string Material { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public string Shape { get; init; } = string.Empty;
    public string Dimensions { get; init; } = string.Empty;
    public string? Details { get; init; }

    public CustomProductDesign ToEntity() => new()
    {
        Type = Type,
        Material = Material,
        Color = Color,
        Shape = Shape,
        Dimensions = Dimensions,
        Details = Details
    };
}