using Application.IntegrationEvents.ProductEvents;
using Infrastructure.Messaging.Consumers.ProductConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers.ProductConsumers;

public class ProductDeletedConsumerTests
{
    private static ReadDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReadDbContext(options);
    }

    private static ILogger<ProductDeletedConsumer> CreateLogger()
    {
        var logger = new Mock<ILogger<ProductDeletedConsumer>>();
        return logger.Object;
    }

    private static Mock<ConsumeContext<ProductDeletedIntegrationEvent>> CreateConsumeContext(string productId)
    {
        var @event = new ProductDeletedIntegrationEvent { ProductId = productId };
        var contextMock = new Mock<ConsumeContext<ProductDeletedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return contextMock;
    }

    private static ProductReadModel CreateProductReadModel(string productId) => new()
    {
        Id = productId,
        Name = "Product To Delete",
        Description = "Description",
        Price = 100,
        PictureUrl = "https://example.com/img.jpg",
        Type = "Resin",
        Brand = "WaxBrand",
        QuantityInStock = 10,
        CreatedAt = DateTime.UtcNow
    };

    private static CustomProductReadModel CreateCustomProductReadModel(string productId) => new()
    {
        Id = productId,
        Name = "Custom Product To Delete",
        Description = "Description",
        Price = 200,
        PictureUrl = "https://example.com/img.jpg",
        TaskId = "task-123",
        GlbUrl = "https://example.com/model.glb",
        OwnerUserId = "user-123",
        Status = "Approved",
        DesignType = "Custom",
        DesignMaterial = "Resin",
        DesignColor = "Red",
        DesignShape = "Round",
        DesignDimensions = "10x10x10",
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Consume_WhenOnlyCatalogProductExists_RemovesProductFromDatabase()
    {
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid().ToString();
        context.Products.Add(CreateProductReadModel(productId));
        await context.SaveChangesAsync();

        var consumer = new ProductDeletedConsumer(context, CreateLogger());

        await consumer.Consume(CreateConsumeContext(productId).Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        product.Should().BeNull();
    }

    [Fact]
    public async Task Consume_WhenOnlyCustomProductExists_RemovesCustomProductFromDatabase()
    {
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid().ToString();
        context.CustomProducts.Add(CreateCustomProductReadModel(productId));
        await context.SaveChangesAsync();

        var consumer = new ProductDeletedConsumer(context, CreateLogger());

        await consumer.Consume(CreateConsumeContext(productId).Object);

        var customProduct = await context.CustomProducts.FirstOrDefaultAsync(p => p.Id == productId);
        customProduct.Should().BeNull();
    }

    [Fact]
    public async Task Consume_WhenBothExist_RemovesBothFromDatabase()
    {
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid().ToString();
        context.Products.Add(CreateProductReadModel(productId));
        context.CustomProducts.Add(CreateCustomProductReadModel(productId));
        await context.SaveChangesAsync();

        var consumer = new ProductDeletedConsumer(context, CreateLogger());

        await consumer.Consume(CreateConsumeContext(productId).Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        var customProduct = await context.CustomProducts.FirstOrDefaultAsync(p => p.Id == productId);
        product.Should().BeNull();
        customProduct.Should().BeNull();
    }

    [Fact]
    public async Task Consume_WhenProductNotFound_DoesNothing()
    {
        using var context = CreateInMemoryContext();
        var consumer = new ProductDeletedConsumer(context, CreateLogger());

        var act = async () => await consumer.Consume(CreateConsumeContext("non-existent").Object);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_OnlyDeletesSpecifiedProduct()
    {
        using var context = CreateInMemoryContext();
        var productIdToDelete = Guid.NewGuid().ToString();
        var productIdToKeep = Guid.NewGuid().ToString();
        context.Products.Add(CreateProductReadModel(productIdToDelete));
        context.Products.Add(CreateProductReadModel(productIdToKeep));
        await context.SaveChangesAsync();

        var consumer = new ProductDeletedConsumer(context, CreateLogger());

        await consumer.Consume(CreateConsumeContext(productIdToDelete).Object);

        var count = await context.Products.CountAsync();
        count.Should().Be(1);
        var remainingProduct = await context.Products.FirstAsync();
        remainingProduct.Id.Should().Be(productIdToKeep);
    }
}
