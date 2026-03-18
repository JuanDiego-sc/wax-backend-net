using ProductDomain = Domain.Entities.Product;

namespace Application.Product.DTOs;

public record CreateProductDto
{

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long Price { get; set; }
    public string? PictureUrl { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int QuantityInStock { get; set; }
    public string? PublicId { get; set; }
    
    public ProductDomain ToEntity() => new()
    {
        Name = Name,
        Description = Description,
        Price = Price,
        PictureUrl = PictureUrl ?? string.Empty,
        Type = Type,
        Brand = Brand,
        QuantityInStock = QuantityInStock,
        PublicId = PublicId
    };

}
