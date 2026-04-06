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

    [Fact]
    public async Task Consume_WhenProductExists_RemovesFromDatabase()
    {
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid().ToString();
        context.Products.Add(CreateProductReadModel(productId));
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new ProductDeletedConsumer(context, logger);
        var @event = new ProductDeletedIntegrationEvent
        {
            ProductId = productId
        };

        var contextMock = new Mock<ConsumeContext<ProductDeletedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        product.Should().BeNull();
    }

    [Fact]
    public async Task Consume_WhenProductNotFound_DoesNothing()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new ProductDeletedConsumer(context, logger);
        var @event = new ProductDeletedIntegrationEvent
        {
            ProductId = "non-existent"
        };

        var contextMock = new Mock<ConsumeContext<ProductDeletedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(contextMock.Object);

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

        var logger = CreateLogger();
        var consumer = new ProductDeletedConsumer(context, logger);
        var @event = new ProductDeletedIntegrationEvent
        {
            ProductId = productIdToDelete
        };

        var contextMock = new Mock<ConsumeContext<ProductDeletedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var count = await context.Products.CountAsync();
        count.Should().Be(1);
        var remainingProduct = await context.Products.FirstAsync();
        remainingProduct.Id.Should().Be(productIdToKeep);
    }
}
