using Application.IntegrationEvents.ProductEvents;
using Infrastructure.Messaging.Consumers.ProductConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers.ProductConsumers;

public class ProductCreatedConsumerTests
{
    private static ReadDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReadDbContext(options);
    }

    private static ILogger<ProductCreatedConsumer> CreateLogger()
    {
        var logger = new Mock<ILogger<ProductCreatedConsumer>>();
        return logger.Object;
    }

    [Fact]
    public async Task Consume_WhenProductDoesNotExist_CreatesReadModel()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new ProductCreatedConsumer(context, logger);
        var productId = Guid.NewGuid().ToString();
        var @event = new ProductCreatedIntegrationEvent
        {
            ProductId = productId,
            Name = "New Product",
            Description = "Description",
            Price = 100,
            PictureUrl = "https://example.com/img.jpg",
            Type = "Resin",
            Brand = "WaxBrand",
            QuantityInStock = 50,
            PublicId = "public-123"
        };

        var contextMock = new Mock<ConsumeContext<ProductCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        product.Should().NotBeNull();
        product!.Name.Should().Be("New Product");
        product.Price.Should().Be(100);
        product.QuantityInStock.Should().Be(50);
    }

    [Fact]
    public async Task Consume_WhenProductAlreadyExists_DoesNotDuplicate()
    {
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid().ToString();
        context.Products.Add(new ProductReadModel
        {
            Id = productId,
            Name = "Existing Product",
            Description = "Existing",
            Price = 200,
            PictureUrl = "https://example.com/old.jpg",
            Type = "Resin",
            Brand = "WaxBrand",
            QuantityInStock = 10,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new ProductCreatedConsumer(context, logger);
        var @event = new ProductCreatedIntegrationEvent
        {
            ProductId = productId,
            Name = "New Product",
            Description = "Description",
            Price = 100,
            PictureUrl = "https://example.com/new.jpg",
            Type = "Resin",
            Brand = "WaxBrand",
            QuantityInStock = 50
        };

        var contextMock = new Mock<ConsumeContext<ProductCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var count = await context.Products.CountAsync();
        count.Should().Be(1);
        var product = await context.Products.FirstAsync(p => p.Id == productId);
        product.Name.Should().Be("Existing Product");
    }

    [Fact]
    public async Task Consume_SetsTimestampsCorrectly()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new ProductCreatedConsumer(context, logger);
        var productId = Guid.NewGuid().ToString();
        var eventTime = DateTime.UtcNow;
        var @event = new ProductCreatedIntegrationEvent
        {
            ProductId = productId,
            Name = "New Product",
            Description = "Description",
            PictureUrl = "https://example.com/img.jpg",
            Type = "Resin",
            Brand = "WaxBrand",
            OccurredAt = eventTime
        };

        var contextMock = new Mock<ConsumeContext<ProductCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstAsync(p => p.Id == productId);
        product.CreatedAt.Should().BeCloseTo(eventTime, TimeSpan.FromSeconds(1));
        product.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
