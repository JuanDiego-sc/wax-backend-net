using Domain.Entities;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Domain;

public class BasketTests
{
    [Fact]
    public void AddItem_WithNewProduct_AddsItemToBasket()
    {
        var basket = BasketFixtures.CreateBasket();
        var product = ProductFixtures.CreateProduct();

        basket.AddItem(product, 3);

        basket.Items.Should().HaveCount(1);
        basket.Items[0].Quantity.Should().Be(3);
    }

    [Fact]
    public void AddItem_WithExistingProduct_IncreasesQuantity()
    {
        var product = ProductFixtures.CreateProduct();
        var basket = BasketFixtures.CreateBasket();
        basket.AddItem(product, 1);

        basket.Items.Should().HaveCount(1);
        basket.Items[0].Quantity.Should().Be(1);
    }

    [Fact]
    public void AddItem_WithNullProduct_ThrowsArgumentNullException()
    {
        var basket = BasketFixtures.CreateBasket();

        var act = () => basket.AddItem(null!, 1);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ThrowsArgumentException()
    {
        var basket = BasketFixtures.CreateBasket();
        var product = ProductFixtures.CreateProduct();

        var act = () => basket.AddItem(product, 0);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Quantity should be greater than zero*");
    }

    [Fact]
    public void AddItem_WithNegativeQuantity_ThrowsArgumentException()
    {
        var basket = BasketFixtures.CreateBasket();
        var product = ProductFixtures.CreateProduct();

        var act = () => basket.AddItem(product, -1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveItem_WhenQuantityExceedsItemQuantity_RemovesItemFromBasket()
    {
        var product = ProductFixtures.CreateProduct();
        var basket = BasketFixtures.CreateBasket();
        basket.AddItem(product, 2);

        basket.RemoveItem(product.Id, 5);

        basket.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_WhenQuantityIsLessThanItemQuantity_DecreasesQuantity()
    {
        var product = ProductFixtures.CreateProduct();
        var basket = BasketFixtures.CreateBasket();
        basket.AddItem(product, 5);

        basket.RemoveItem(product.Id, 3);

        basket.Items.Should().HaveCount(1);
        basket.Items[0].Quantity.Should().Be(2);
    }

    [Fact]
    public void RemoveItem_ForNonExistentProduct_DoesNotThrow()
    {
        var basket = BasketFixtures.CreateBasket();

        var act = () => basket.RemoveItem("non-existent-id", 1);

        act.Should().NotThrow();
        basket.Items.Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_WithZeroQuantity_ThrowsArgumentException()
    {
        var product = ProductFixtures.CreateProduct();
        var basket = BasketFixtures.CreateBasket();
        basket.AddItem(product, 2);

        var act = () => basket.RemoveItem(product.Id, 0);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Quantity should be greater than zero*");
    }
}
