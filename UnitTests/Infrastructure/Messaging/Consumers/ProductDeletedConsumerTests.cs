using Application.IntegrationEvents.ProductEvents;
using Infrastructure.Messaging.Consumers;
using Infrastructure.Messaging.Consumers.ProductConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers;

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

    [Fact]
    public async Task Consume_WhenProductExists_RemovesProduct()
    {
        using var context = CreateInMemoryContext();
        var existingProduct = new ProductReadModel
        {
            Id = "delete-test",
            Name = "To Delete",
            Description = "Desc",
            Price = 500,
            PictureUrl = "url",
            Type = "Type",
            Brand = "Brand",
            QuantityInStock = 5,
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
        context.Products.Add(existingProduct);
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new ProductDeletedConsumer(context, logger);
        var @event = new ProductDeletedIntegrationEvent
        {
            ProductId = "delete-test",
            OccurredAt = DateTime.UtcNow
        };

        var contextMock = new Mock<ConsumeContext<ProductDeletedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == "delete-test");
        product.Should().BeNull();
    }

    [Fact]
    public async Task Consume_WhenProductDoesNotExist_DoesNotThrowException()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new ProductDeletedConsumer(context, logger);
        var @event = new ProductDeletedIntegrationEvent
        {
            ProductId = "non-existent",
            OccurredAt = DateTime.UtcNow
        };

        var contextMock = new Mock<ConsumeContext<ProductDeletedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(contextMock.Object);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_WhenProductExists_OnlyDeletesSpecifiedProduct()
    {
        using var context = CreateInMemoryContext();
        var product1 = new ProductReadModel
        {
            Id = "keep-this",
            Name = "Keep",
            Description = "Desc",
            Price = 500,
            PictureUrl = "url",
            Type = "Type",
            Brand = "Brand",
            QuantityInStock = 5,
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
        var product2 = new ProductReadModel
        {
            Id = "delete-this",
            Name = "Delete",
            Description = "Desc",
            Price = 500,
            PictureUrl = "url",
            Type = "Type",
            Brand = "Brand",
            QuantityInStock = 5,
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
        context.Products.AddRange(product1, product2);
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new ProductDeletedConsumer(context, logger);
        var @event = new ProductDeletedIntegrationEvent
        {
            ProductId = "delete-this",
            OccurredAt = DateTime.UtcNow
        };

        var contextMock = new Mock<ConsumeContext<ProductDeletedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var remainingProducts = await context.Products.ToListAsync();
        remainingProducts.Should().HaveCount(1);
        remainingProducts[0].Id.Should().Be("keep-this");
    }
}
