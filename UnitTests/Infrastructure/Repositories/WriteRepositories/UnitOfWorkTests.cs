using Domain.Entities;
using Infrastructure.Repositories.WriteRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace UnitTests.Infrastructure.Repositories.WriteRepositories;

public class UnitOfWorkTests
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
    public async Task CompleteAsync_WhenChangesExist_ReturnsTrue()
    {
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        
        context.Products.Add(CreateProduct());

        var result = await unitOfWork.CompleteAsync();

        result.Should().BeTrue();
    }

    [Fact]
    public async Task CompleteAsync_WhenNoChanges_ReturnsFalse()
    {
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);

        var result = await unitOfWork.CompleteAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CompleteAsync_PersistsChangesToDatabase()
    {
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        var product = CreateProduct();
        context.Products.Add(product);

        await unitOfWork.CompleteAsync();

        var stored = await context.Products.FindAsync(product.Id);
        stored.Should().NotBeNull();
    }

    [Fact]
    public void HasChanges_WhenChangesExist_ReturnsTrue()
    {
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        context.Products.Add(CreateProduct());

        var result = unitOfWork.HasChanges();

        result.Should().BeTrue();
    }

    [Fact]
    public void HasChanges_WhenNoChanges_ReturnsFalse()
    {
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);

        var result = unitOfWork.HasChanges();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasChanges_AfterComplete_ReturnsFalse()
    {
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        context.Products.Add(CreateProduct());

        unitOfWork.HasChanges().Should().BeTrue();
        await unitOfWork.CompleteAsync();
        
        unitOfWork.HasChanges().Should().BeFalse();
    }

    [Fact]
    public async Task CompleteAsync_SupportsCancellationToken()
    {
        using var context = CreateInMemoryContext();
        var unitOfWork = new UnitOfWork(context);
        context.Products.Add(CreateProduct());
        var cts = new CancellationTokenSource();

        var result = await unitOfWork.CompleteAsync(cts.Token);

        result.Should().BeTrue();
    }
}
