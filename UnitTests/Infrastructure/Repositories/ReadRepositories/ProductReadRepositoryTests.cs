using Infrastructure.Repositories.ReadRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Repositories.ReadRepositories;

public class ProductReadRepositoryTests
{
    private ReadDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReadDbContext(options);
    }

    private ProductReadModel CreateReadModel(string id, string name) => new()
    {
        Id = id,
        Name = name,
        Description = "Description",
        Price = 1000,
        PictureUrl = "https://cdn/img.jpg",
        Type = "Resin",
        Brand = "Brand",
        QuantityInStock = 10,
        PublicId = "public123",
        CreatedAt = DateTime.UtcNow,
        LastSyncedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProductDto()
    {
        using var context = CreateInMemoryContext();
        var product = CreateReadModel("test-id", "Test Product");
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repository = new ProductReadRepository(context);
        var result = await repository.GetByIdAsync("test-id");

        result.Should().NotBeNull();
        result!.Id.Should().Be("test-id");
        result.Name.Should().Be("Test Product");
        result.Description.Should().Be("Description");
        result.Price.Should().Be(1000);
        result.PictureUrl.Should().Be("https://cdn/img.jpg");
        result.Type.Should().Be("Resin");
        result.Brand.Should().Be("Brand");
        result.QuantityInStock.Should().Be(10);
        result.PublicId.Should().Be("public123");
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        using var context = CreateInMemoryContext();
        var repository = new ProductReadRepository(context);

        var result = await repository.GetByIdAsync("non-existent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithMultipleProducts_ReturnsCorrectProduct()
    {
        using var context = CreateInMemoryContext();
        context.Products.AddRange(
            CreateReadModel("id1", "Product 1"),
            CreateReadModel("id2", "Product 2"),
            CreateReadModel("id3", "Product 3")
        );
        await context.SaveChangesAsync();

        var repository = new ProductReadRepository(context);
        var result = await repository.GetByIdAsync("id2");

        result.Should().NotBeNull();
        result!.Id.Should().Be("id2");
        result.Name.Should().Be("Product 2");
    }

    [Fact]
    public async Task GetByIdAsync_WithCancellationToken_UsesCancellationToken()
    {
        using var context = CreateInMemoryContext();
        var product = CreateReadModel("test-id", "Test Product");
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repository = new ProductReadRepository(context);
        var cts = new CancellationTokenSource();

        var result = await repository.GetByIdAsync("test-id", cts.Token);

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetQueryable_ReturnsQueryableOfProductDto()
    {
        using var context = CreateInMemoryContext();
        context.Products.AddRange(
            CreateReadModel("id1", "Product 1"),
            CreateReadModel("id2", "Product 2")
        );
        context.SaveChanges();

        var repository = new ProductReadRepository(context);
        var queryable = repository.GetQueryable();

        queryable.Should().NotBeNull();
        var results = queryable.ToList();
        results.Should().HaveCount(2);
        results[0].Name.Should().Be("Product 1");
        results[1].Name.Should().Be("Product 2");
    }

    [Fact]
    public void GetQueryable_WithNoProducts_ReturnsEmptyQueryable()
    {
        using var context = CreateInMemoryContext();
        var repository = new ProductReadRepository(context);

        var queryable = repository.GetQueryable();

        queryable.Should().NotBeNull();
        queryable.ToList().Should().BeEmpty();
    }

    [Fact]
    public void GetQueryable_MapsAllPropertiesCorrectly()
    {
        using var context = CreateInMemoryContext();
        var product = CreateReadModel("test-id", "Test Product");
        product.PublicId = "test-public-id";
        context.Products.Add(product);
        context.SaveChanges();

        var repository = new ProductReadRepository(context);
        var queryable = repository.GetQueryable();
        var result = queryable.FirstOrDefault();

        result.Should().NotBeNull();
        result!.Id.Should().Be("test-id");
        result.Name.Should().Be("Test Product");
        result.Description.Should().Be("Description");
        result.Price.Should().Be(1000);
        result.PictureUrl.Should().Be("https://cdn/img.jpg");
        result.Type.Should().Be("Resin");
        result.Brand.Should().Be("Brand");
        result.QuantityInStock.Should().Be(10);
        result.PublicId.Should().Be("test-public-id");
    }

    [Fact]
    public void GetQueryable_CanBeUsedWithLinqOperations()
    {
        using var context = CreateInMemoryContext();
        context.Products.AddRange(
            CreateReadModel("id1", "Candle"),
            CreateReadModel("id2", "Wax"),
            CreateReadModel("id3", "Candle Holder")
        );
        context.SaveChanges();

        var repository = new ProductReadRepository(context);
        var queryable = repository.GetQueryable();

        var filtered = queryable.Where(p => p.Name.Contains("Candle")).ToList();

        filtered.Should().HaveCount(2);
        filtered.Should().Contain(p => p.Name == "Candle");
        filtered.Should().Contain(p => p.Name == "Candle Holder");
    }
}
