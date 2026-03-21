using ProductDomain = Domain.Entities.Product;

namespace Application.Product.DTOs;

public class UpdateProductDto
{
    public required string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long Price { get; set; }
    public string? PictureUrl { get; set; }
    public string  Type { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int QuantityInStock { get; set; }
    public string? PublicId { get; set; }
    
    public void ApplyTo(ProductDomain product)
    {
        product.Name = Name;
        product.Description = Description;
        product.Price = Price;
        product.PictureUrl = PictureUrl ?? product.PictureUrl;
        product.Type = Type;
        product.Brand = Brand;
        product.QuantityInStock = QuantityInStock;
        product.PublicId = PublicId;
        product.UpdatedAt = DateTime.UtcNow;
    }
}
