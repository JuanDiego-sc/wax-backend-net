using Application.IntegrationEvents.ProductEvents;
using Infrastructure.Messaging.Consumers.ProductConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers.ProductConsumers;

public class ProductStockChangedConsumerTests
{
    private static ReadDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReadDbContext(options);
    }

    private static ILogger<ProductStockChangedConsumer> CreateLogger()
    {
        var logger = new Mock<ILogger<ProductStockChangedConsumer>>();
        return logger.Object;
    }

    private static ProductReadModel CreateProductReadModel(string productId) => new()
    {
        Id = productId,
        Name = "Test Product",
        Description = "Test Description",
        Price = 1000,
        PictureUrl = "https://example.com/image.jpg",
        Type = "Resin",
        Brand = "WaxBrand",
        QuantityInStock = 10,
        CreatedAt = DateTime.UtcNow,
        LastSyncedAt = DateTime.UtcNow.AddMinutes(-5)
    };

    [Fact]
    public async Task Consume_WhenProductExists_UpdatesStock()
    {
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid().ToString();
        context.Products.Add(CreateProductReadModel(productId));
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new ProductStockChangedConsumer(context, logger);
        var @event = new ProductStockChangedIntegrationEvent
        {
            ProductId = productId,
            NewQuantity = 25
        };

        var contextMock = new Mock<ConsumeContext<ProductStockChangedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        product.Should().NotBeNull();
        product!.QuantityInStock.Should().Be(25);
    }

    [Fact]
    public async Task Consume_WhenProductNotFound_ThrowsException()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new ProductStockChangedConsumer(context, logger);
        var @event = new ProductStockChangedIntegrationEvent
        {
            ProductId = "non-existent",
            NewQuantity = 50
        };

        var contextMock = new Mock<ConsumeContext<ProductStockChangedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(contextMock.Object);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Product read model not found");
    }

    [Fact]
    public async Task Consume_UpdatesTimestamps()
    {
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid().ToString();
        context.Products.Add(CreateProductReadModel(productId));
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new ProductStockChangedConsumer(context, logger);
        var eventTime = DateTime.UtcNow;
        var @event = new ProductStockChangedIntegrationEvent
        {
            ProductId = productId,
            NewQuantity = 15,
            OccurredAt = eventTime
        };

        var contextMock = new Mock<ConsumeContext<ProductStockChangedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        product!.UpdatedAt.Should().BeCloseTo(eventTime, TimeSpan.FromSeconds(1));
        product.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Consume_DoesNotModifyOtherFields()
    {
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid().ToString();
        context.Products.Add(CreateProductReadModel(productId));
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new ProductStockChangedConsumer(context, logger);
        var @event = new ProductStockChangedIntegrationEvent
        {
            ProductId = productId,
            NewQuantity = 100
        };

        var contextMock = new Mock<ConsumeContext<ProductStockChangedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        product!.Name.Should().Be("Test Product");
        product.Price.Should().Be(1000);
        product.Description.Should().Be("Test Description");
    }
}
