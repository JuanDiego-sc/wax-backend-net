using Domain.Entities;
using Domain.OrderAggregate;

namespace UnitTests.Helpers.Fixtures;

public static class ProductFixtures
{
    public static Product CreateProduct(
        string? id = null,
        string name = "Test Product",
        string description = "A test product description",
        long price = 1500,
        string pictureUrl = "https://example.com/image.jpg",
        string type = "Resin",
        string brand = "WaxBrand",
        int quantityInStock = 10,
        string? publicId = null)
    {
        return new Product
        {
            Name = name,
            Description = description,
            Price = price,
            PictureUrl = pictureUrl,
            Type = type,
            Brand = brand,
            QuantityInStock = quantityInStock,
            PublicId = publicId
        };
    }

    public static ProductOrderItem CreateProductOrderItem(string? productId = null, string name = "Test Product")
    {
        return new ProductOrderItem
        {
            ProductId = productId ?? Guid.NewGuid().ToString(),
            Name = name
        };
    }
}
