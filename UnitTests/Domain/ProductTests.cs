using Domain.Entities;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void Product_WithRequiredProperties_SetsPropertiesCorrectly()
    {
        var product = ProductFixtures.CreateProduct(
            name: "Candle",
            description: "A scented candle",
            price: 2500,
            type: "Candle",
            brand: "WaxCo");

        product.Name.Should().Be("Candle");
        product.Description.Should().Be("A scented candle");
        product.Price.Should().Be(2500);
        product.Type.Should().Be("Candle");
        product.Brand.Should().Be("WaxCo");
    }

    [Fact]
    public void Product_WhenCreated_AssignsNonEmptyId()
    {
        var product = new Product
        {
            Name = "Test",
            Description = "Desc",
            Price = 1000,
            PictureUrl = "https://example.com/img.jpg",
            Type = "Resin",
            Brand = "Brand"
        };

        product.Id.Should().NotBeNullOrEmpty();
    }
}
