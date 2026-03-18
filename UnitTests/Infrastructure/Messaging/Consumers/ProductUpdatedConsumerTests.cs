using Application.IntegrationEvents.ProductEvents;
using Infrastructure.Messaging.Consumers;
using Infrastructure.Messaging.Consumers.ProductConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers;

public class ProductUpdatedConsumerTests
{
    private static ReadDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReadDbContext(options);
    }

    private ProductUpdatedIntegrationEvent CreateEvent(string productId = "pid1", string name = "Updated Product") => new()
    {
        ProductId = productId,
        Name = name,
        Description = "Updated Description",
        Price = 2000,
        PictureUrl = "https://cdn/updated.jpg",
        Type = "Updated Type",
        Brand = "Updated Brand",
        QuantityInStock = 20,
        PublicId = "updated-public",
        OccurredAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Consume_WhenProductExists_UpdatesExistingProduct()
    {
        using var context = CreateInMemoryContext();
        var existingProduct = new ProductReadModel
        {
            Id = "update-test",
            Name = "Old Name",
            Description = "Old Description",
            Price = 500,
            PictureUrl = "old-url",
            Type = "Old Type",
            Brand = "Old Brand",
            QuantityInStock = 5,
            PublicId = "old-public",
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            LastSyncedAt = DateTime.UtcNow.AddDays(-1)
        };
        context.Products.Add(existingProduct);
        await context.SaveChangesAsync();

        var consumer = new ProductUpdatedConsumer(context);
        var @event = CreateEvent(productId: "update-test", name: "New Name");

        var contextMock = new Mock<ConsumeContext<ProductUpdatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == "update-test");
        product.Should().NotBeNull();
        product!.Name.Should().Be("New Name");
        product.Description.Should().Be("Updated Description");
        product.Price.Should().Be(2000);
        product.PictureUrl.Should().Be("https://cdn/updated.jpg");
        product.Type.Should().Be("Updated Type");
        product.Brand.Should().Be("Updated Brand");
        product.QuantityInStock.Should().Be(20);
        product.PublicId.Should().Be("updated-public");
        product.UpdatedAt.Should().BeCloseTo(@event.OccurredAt, TimeSpan.FromSeconds(1));
        product.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Consume_WhenProductDoesNotExist_CreatesNewProduct()
    {
        using var context = CreateInMemoryContext();
        var consumer = new ProductUpdatedConsumer(context);
        var @event = CreateEvent(productId: "new-product", name: "New Product");

        var contextMock = new Mock<ConsumeContext<ProductUpdatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == "new-product");
        product.Should().NotBeNull();
        product!.Name.Should().Be("New Product");
        product.Description.Should().Be("Updated Description");
        product.Price.Should().Be(2000);
        product.CreatedAt.Should().BeCloseTo(@event.OccurredAt, TimeSpan.FromSeconds(1));
        product.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Consume_WhenUpdatingProduct_DoesNotChangeCreatedAt()
    {
        using var context = CreateInMemoryContext();
        var originalCreatedAt = DateTime.UtcNow.AddDays(-10);
        var existingProduct = new ProductReadModel
        {
            Id = "timestamp-test",
            Name = "Original",
            Description = "Desc",
            Price = 500,
            PictureUrl = "url",
            Type = "Type",
            Brand = "Brand",
            QuantityInStock = 5,
            CreatedAt = originalCreatedAt,
            LastSyncedAt = DateTime.UtcNow.AddDays(-1)
        };
        context.Products.Add(existingProduct);
        await context.SaveChangesAsync();

        var consumer = new ProductUpdatedConsumer(context);
        var @event = CreateEvent(productId: "timestamp-test");

        var contextMock = new Mock<ConsumeContext<ProductUpdatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == "timestamp-test");
        product!.CreatedAt.Should().Be(originalCreatedAt);
    }
}
