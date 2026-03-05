using Application.Product.Extensions;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Product;

public class ProductExtensionsTests
{
    [Fact]
    public void ToDto_MapsAllPropertiesCorrectly()
    {
        var product = ProductFixtures.CreateProduct(
            name: "Wax Ring",
            description: "A nice ring",
            price: 2000,
            pictureUrl: "https://img.com/ring.jpg",
            type: "Jewelry",
            brand: "WaxCo",
            quantityInStock: 7);

        var dto = product.ToDto();

        dto.Name.Should().Be("Wax Ring");
        dto.Description.Should().Be("A nice ring");
        dto.Price.Should().Be(2000);
        dto.PictureUrl.Should().Be("https://img.com/ring.jpg");
        dto.Type.Should().Be("Jewelry");
        dto.Brand.Should().Be("WaxCo");
        dto.QuantityInStock.Should().Be(7);
    }

    [Fact]
    public void Sort_ByPrice_OrdersAscending()
    {
        var products = new List<global::Domain.Entities.Product>
        {
            ProductFixtures.CreateProduct(price: 3000),
            ProductFixtures.CreateProduct(price: 1000),
            ProductFixtures.CreateProduct(price: 2000),
        }.AsQueryable();

        var sorted = products.Sort("price").ToList();

        sorted[0].Price.Should().Be(1000);
        sorted[1].Price.Should().Be(2000);
        sorted[2].Price.Should().Be(3000);
    }

    [Fact]
    public void Sort_ByPriceDesc_OrdersDescending()
    {
        var products = new List<global::Domain.Entities.Product>
        {
            ProductFixtures.CreateProduct(price: 1000),
            ProductFixtures.CreateProduct(price: 3000),
            ProductFixtures.CreateProduct(price: 2000),
        }.AsQueryable();

        var sorted = products.Sort("priceDesc").ToList();

        sorted[0].Price.Should().Be(3000);
        sorted[2].Price.Should().Be(1000);
    }

    [Fact]
    public void Sort_DefaultOrUnknownKey_OrdersByName()
    {
        var products = new List<global::Domain.Entities.Product>
        {
            ProductFixtures.CreateProduct(name: "Zebra"),
            ProductFixtures.CreateProduct(name: "Apple"),
            ProductFixtures.CreateProduct(name: "Mango"),
        }.AsQueryable();

        var sorted = products.Sort(null).ToList();

        sorted[0].Name.Should().Be("Apple");
    }

    [Fact]
    public void Search_WhenEmpty_ReturnsAll()
    {
        var products = new List<global::Domain.Entities.Product>
        {
            ProductFixtures.CreateProduct(name: "Alpha"),
            ProductFixtures.CreateProduct(name: "Beta"),
        }.AsQueryable();

        var result = products.Search(null).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public void Search_WithTerm_FiltersMatchingNames()
    {
        var products = new List<global::Domain.Entities.Product>
        {
            ProductFixtures.CreateProduct(name: "Candle"),
            ProductFixtures.CreateProduct(name: "Wax Seal"),
            ProductFixtures.CreateProduct(name: "Candle Holder"),
        }.AsQueryable();

        var result = products.Search("candle").ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public void Filter_WithBrandFilter_FiltersCorrectly()
    {
        var products = new List<global::Domain.Entities.Product>
        {
            ProductFixtures.CreateProduct(brand: "WaxCo"),
            ProductFixtures.CreateProduct(brand: "OtherBrand"),
        }.AsQueryable();

        var result = products.Filter(brands: "waxco", types: null).ToList();

        result.Should().HaveCount(1);
        result[0].Brand.Should().Be("WaxCo");
    }

    [Fact]
    public void Filter_WithTypeFilter_FiltersCorrectly()
    {
        var products = new List<global::Domain.Entities.Product>
        {
            ProductFixtures.CreateProduct(type: "Candle"),
            ProductFixtures.CreateProduct(type: "Jewelry"),
            ProductFixtures.CreateProduct(type: "Candle"),
        }.AsQueryable();

        var result = products.Filter(brands: null, types: "candle").ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public void Filter_WithNoFilters_ReturnsAll()
    {
        var products = new List<global::Domain.Entities.Product>
        {
            ProductFixtures.CreateProduct(),
            ProductFixtures.CreateProduct(),
        }.AsQueryable();

        var result = products.Filter(null, null).ToList();

        result.Should().HaveCount(2);
    }
}
