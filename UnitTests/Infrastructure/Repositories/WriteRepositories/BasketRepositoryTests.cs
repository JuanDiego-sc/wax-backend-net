using Domain.Entities;
using Infrastructure.Repositories.WriteRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace UnitTests.Infrastructure.Repositories.WriteRepositories;

public class BasketRepositoryTests
{
    private static WriteDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new WriteDbContext(options);
    }

    private static Product CreateProduct(string? id = null) => new()
    {
        Id = id ?? Guid.NewGuid().ToString(),
        Name = "Test Product",
        Description = "Test Description",
        Price = 100,
        PictureUrl = "https://example.com/img.jpg",
        Type = "Resin",
        Brand = "WaxBrand",
        QuantityInStock = 10
    };

    private static Basket CreateBasket(string? basketId = null) => new()
    {
        BasketId = basketId ?? Guid.NewGuid().ToString()
    };

    [Fact]
    public async Task GetBasketWithItemsAsync_WhenBasketExists_ReturnsBasketWithItems()
    {
        using var context = CreateInMemoryContext();
        var product = CreateProduct();
        var basket = CreateBasket();
        basket.Items.Add(new BasketItem
        {
            ProductId = product.Id,
            Product = product,
            Quantity = 2
        });
        context.Products.Add(product);
        context.Baskets.Add(basket);
        await context.SaveChangesAsync();

        var repository = new BasketRepository(context);

        var result = await repository.GetBasketWithItemsAsync(basket.BasketId);

        result.Should().NotBeNull();
        result!.BasketId.Should().Be(basket.BasketId);
        result.Items.Should().HaveCount(1);
        result.Items[0].Product.Should().NotBeNull();
    }

    [Fact]
    public async Task GetBasketWithItemsAsync_WhenBasketDoesNotExist_ReturnsNull()
    {
        using var context = CreateInMemoryContext();
        var repository = new BasketRepository(context);

        var result = await repository.GetBasketWithItemsAsync("non-existent-basket");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBasketWithItemsAsync_WhenBasketIdIsNull_ReturnsNull()
    {
        using var context = CreateInMemoryContext();
        var repository = new BasketRepository(context);

        var result = await repository.GetBasketWithItemsAsync(null);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBasketWithItemsAsync_WhenBasketIdIsEmpty_ReturnsNull()
    {
        using var context = CreateInMemoryContext();
        var repository = new BasketRepository(context);

        var result = await repository.GetBasketWithItemsAsync(string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Add_AddsBasketToContext()
    {
        using var context = CreateInMemoryContext();
        var repository = new BasketRepository(context);
        var basket = CreateBasket();

        repository.Add(basket);
        await context.SaveChangesAsync();

        var stored = await context.Baskets.FirstOrDefaultAsync(b => b.BasketId == basket.BasketId);
        stored.Should().NotBeNull();
    }

    [Fact]
    public async Task Remove_RemovesBasketFromContext()
    {
        using var context = CreateInMemoryContext();
        var basket = CreateBasket();
        context.Baskets.Add(basket);
        await context.SaveChangesAsync();

        var repository = new BasketRepository(context);

        repository.Remove(basket);
        await context.SaveChangesAsync();

        var stored = await context.Baskets.FirstOrDefaultAsync(b => b.BasketId == basket.BasketId);
        stored.Should().BeNull();
    }

    [Fact]
    public async Task GetBasketWithItemsAsync_IncludesProductsInItems()
    {
        using var context = CreateInMemoryContext();
        var product1 = CreateProduct();
        var product2 = CreateProduct();
        var basket = CreateBasket();
        basket.Items.Add(new BasketItem { ProductId = product1.Id, Product = product1, Quantity = 1 });
        basket.Items.Add(new BasketItem { ProductId = product2.Id, Product = product2, Quantity = 3 });
        context.Products.Add(product1);
        context.Products.Add(product2);
        context.Baskets.Add(basket);
        await context.SaveChangesAsync();

        var repository = new BasketRepository(context);

        var result = await repository.GetBasketWithItemsAsync(basket.BasketId);

        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(item => item.Product.Should().NotBeNull());
    }
}
