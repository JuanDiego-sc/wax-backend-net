using Domain.Entities;
using Infrastructure.Repositories.WriteRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace UnitTests.Infrastructure.Repositories.WriteRepositories;

public class ProductRepositoryTests
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

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProduct()
    {
        using var context = CreateInMemoryContext();
        var product = CreateProduct();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        var result = await repository.GetByIdAsync(product.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(product.Id);
        result.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);

        var result = await repository.GetByIdAsync("non-existent-id");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetQueryable_ReturnsAllProducts()
    {
        using var context = CreateInMemoryContext();
        context.Products.Add(CreateProduct());
        context.Products.Add(CreateProduct());
        context.Products.Add(CreateProduct());
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        var result = await repository.GetQueryable().ToListAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task Add_AddsProductToContext()
    {
        using var context = CreateInMemoryContext();
        var repository = new ProductRepository(context);
        var product = CreateProduct();

        repository.Add(product);
        await context.SaveChangesAsync();

        var stored = await context.Products.FindAsync(product.Id);
        stored.Should().NotBeNull();
        stored!.Name.Should().Be("Test Product");
    }

    [Fact]
    public async Task Remove_RemovesProductFromContext()
    {
        using var context = CreateInMemoryContext();
        var product = CreateProduct();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);

        repository.Remove(product);
        await context.SaveChangesAsync();

        var stored = await context.Products.FindAsync(product.Id);
        stored.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_SupportsCancellationToken()
    {
        using var context = CreateInMemoryContext();
        var product = CreateProduct();
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repository = new ProductRepository(context);
        var cts = new CancellationTokenSource();

        var result = await repository.GetByIdAsync(product.Id, cts.Token);

        result.Should().NotBeNull();
    }
}
