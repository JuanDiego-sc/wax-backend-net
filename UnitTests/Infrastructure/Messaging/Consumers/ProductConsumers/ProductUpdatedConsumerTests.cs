using Application.IntegrationEvents.ProductEvents;
using Infrastructure.Messaging.Consumers.ProductConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers.ProductConsumers;

public class ProductUpdatedConsumerTests
{
    private static ReadDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReadDbContext(options);
    }

    private static ILogger<ProductUpdatedConsumer> CreateLogger()
    {
        var logger = new Mock<ILogger<ProductUpdatedConsumer>>();
        return logger.Object;
    }

    private static ProductReadModel CreateProductReadModel(string productId) => new()
    {
        Id = productId,
        Name = "Original Name",
        Description = "Original Description",
        Price = 100,
        PictureUrl = "https://example.com/original.jpg",
        Type = "Resin",
        Brand = "WaxBrand",
        QuantityInStock = 10,
        PublicId = "original-public-id",
        CreatedAt = DateTime.UtcNow.AddDays(-1),
        LastSyncedAt = DateTime.UtcNow.AddMinutes(-5)
    };

    [Fact]
    public async Task Consume_WhenProductExists_UpdatesAllFields()
    {
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid().ToString();
        context.Products.Add(CreateProductReadModel(productId));
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new ProductUpdatedConsumer(context, logger);
        var @event = new ProductUpdatedIntegrationEvent
        {
            ProductId = productId,
            Name = "Updated Name",
            Description = "Updated Description",
            Price = 200,
            PictureUrl = "https://example.com/updated.jpg",
            Type = "Filament",
            Brand = "NewBrand",
            QuantityInStock = 25,
            PublicId = "new-public-id"
        };

        var contextMock = new Mock<ConsumeContext<ProductUpdatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        product.Should().NotBeNull();
        product!.Name.Should().Be("Updated Name");
        product.Description.Should().Be("Updated Description");
        product.Price.Should().Be(200);
        product.PictureUrl.Should().Be("https://example.com/updated.jpg");
        product.Type.Should().Be("Filament");
        product.Brand.Should().Be("NewBrand");
        product.QuantityInStock.Should().Be(25);
        product.PublicId.Should().Be("new-public-id");
    }

    [Fact]
    public async Task Consume_WhenProductNotFound_CreatesNewOne()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new ProductUpdatedConsumer(context, logger);
        var productId = Guid.NewGuid().ToString();
        var @event = new ProductUpdatedIntegrationEvent
        {
            ProductId = productId,
            Name = "New Product",
            Description = "Created via update",
            Price = 150,
            PictureUrl = "https://example.com/new.jpg",
            Type = "Resin",
            Brand = "WaxBrand",
            QuantityInStock = 30,
            PublicId = "new-id"
        };

        var contextMock = new Mock<ConsumeContext<ProductUpdatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId);
        product.Should().NotBeNull();
        product!.Name.Should().Be("New Product");
        product.Price.Should().Be(150);
    }

    [Fact]
    public async Task Consume_UpdatesTimestamps()
    {
        using var context = CreateInMemoryContext();
        var productId = Guid.NewGuid().ToString();
        context.Products.Add(CreateProductReadModel(productId));
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new ProductUpdatedConsumer(context, logger);
        var eventTime = DateTime.UtcNow;
        var @event = new ProductUpdatedIntegrationEvent
        {
            ProductId = productId,
            Name = "Updated",
            Description = "Updated Description",
            PictureUrl = "https://example.com/updated.jpg",
            Type = "Resin",
            Brand = "WaxBrand",
            OccurredAt = eventTime
        };

        var contextMock = new Mock<ConsumeContext<ProductUpdatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstAsync(p => p.Id == productId);
        product.UpdatedAt.Should().BeCloseTo(eventTime, TimeSpan.FromSeconds(1));
        product.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
