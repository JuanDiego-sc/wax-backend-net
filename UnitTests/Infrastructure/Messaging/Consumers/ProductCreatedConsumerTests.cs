using Application.IntegrationEvents.ProductEvents;
using Infrastructure.Messaging.Consumers.ProductConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers;

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

    private ProductCreatedIntegrationEvent CreateEvent(string productId = "pid1", string name = "Product") => new()
    {
        ProductId = productId,
        Name = name,
        Description = "Description",
        Price = 1000,
        PictureUrl = "https://cdn/img.jpg",
        Type = "Resin",
        Brand = "Brand",
        QuantityInStock = 10,
        PublicId = "public123",
        OccurredAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Consume_WhenProductDoesNotExist_AddsNewProductReadModel()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new ProductCreatedConsumer(context, logger);
        var @event = CreateEvent(productId: "new-product", name: "New Product");

        var contextMock = new Mock<ConsumeContext<ProductCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == "new-product");
        product.Should().NotBeNull();
        product!.Name.Should().Be("New Product");
        product.Description.Should().Be("Description");
        product.Price.Should().Be(1000);
        product.PictureUrl.Should().Be("https://cdn/img.jpg");
        product.Type.Should().Be("Resin");
        product.Brand.Should().Be("Brand");
        product.QuantityInStock.Should().Be(10);
        product.PublicId.Should().Be("public123");
        product.CreatedAt.Should().BeCloseTo(@event.OccurredAt, TimeSpan.FromSeconds(1));
        product.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Consume_WhenProductAlreadyExists_DoesNotAddDuplicate()
    {
        using var context = CreateInMemoryContext();
        var existingProduct = new ProductReadModel
        {
            Id = "existing-id",
            Name = "Existing",
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
        var consumer = new ProductCreatedConsumer(context, logger);
        var @event = CreateEvent(productId: "existing-id", name: "Updated Name");

        var contextMock = new Mock<ConsumeContext<ProductCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var products = await context.Products.Where(p => p.Id == "existing-id").ToListAsync();
        products.Should().HaveCount(1);
        products[0].Name.Should().Be("Existing");
    }

    [Fact]
    public async Task Consume_WhenEventHasNullPublicId_CreatesProductWithNullPublicId()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new ProductCreatedConsumer(context, logger);
        var @event = CreateEvent(productId: "nullpublicid-test");
        @event.PublicId = null;

        var contextMock = new Mock<ConsumeContext<ProductCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var product = await context.Products.FirstOrDefaultAsync(p => p.Id == "nullpublicid-test");
        product.Should().NotBeNull();
        product!.PublicId.Should().BeNull();
    }
}
