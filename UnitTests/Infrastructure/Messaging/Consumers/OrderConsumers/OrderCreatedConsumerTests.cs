using Application.IntegrationEvents.OrderEvents;
using Infrastructure.Messaging.Consumers.OrderConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers.OrderConsumers;

public class OrderCreatedConsumerTests
{
    private static ReadDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReadDbContext(options);
    }

    private static ILogger<OrderCreatedConsumer> CreateLogger()
    {
        var logger = new Mock<ILogger<OrderCreatedConsumer>>();
        return logger.Object;
    }

    private static OrderCreatedIntegrationEvent CreateEvent(string? orderId = null) => new()
    {
        OrderId = orderId ?? Guid.NewGuid().ToString(),
        BuyerEmail = "buyer@test.com",
        OrderStatus = "Pending",
        Subtotal = 5000,
        DeliveryFee = 500,
        Total = 5500,
        BillingName = "Test User",
        BillingLine1 = "123 Test St",
        BillingLine2 = null,
        BillingCity = "Test City",
        BillingState = "TS",
        BillingPostalCode = "12345",
        BillingCountry = "US",
        PaymentLast4 = 4242,
        PaymentBrand = "Visa",
        PaymentExpMonth = 12,
        PaymentExpYear = 2026,
        OrderItems = "[]",
        PaymentIntentId = "pi_test",
        OccurredAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Consume_WhenOrderDoesNotExist_AddsNewOrderReadModel()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new OrderCreatedConsumer(context, logger);
        var @event = CreateEvent(orderId: "new-order");

        var contextMock = new Mock<ConsumeContext<OrderCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == "new-order");
        order.Should().NotBeNull();
        order!.BuyerEmail.Should().Be("buyer@test.com");
        order.OrderStatus.Should().Be("Pending");
        order.Subtotal.Should().Be(5000);
        order.DeliveryFee.Should().Be(500);
        order.Total.Should().Be(5500);
    }

    [Fact]
    public async Task Consume_WhenOrderAlreadyExists_DoesNotAddDuplicate()
    {
        using var context = CreateInMemoryContext();
        var existingOrder = new OrderReadModel
        {
            Id = "existing-order",
            BuyerEmail = "original@test.com",
            OrderStatus = "Completed",
            Subtotal = 1000,
            DeliveryFee = 100,
            Total = 1100,
            BillingName = "Original",
            BillingLine1 = "Original St",
            BillingCity = "Original City",
            BillingState = "OR",
            BillingPostalCode = "11111",
            BillingCountry = "US",
            PaymentLast4 = 1234,
            PaymentBrand = "Mastercard",
            PaymentExpMonth = 1,
            PaymentExpYear = 2025,
            OrderItems = "[]",
            PaymentIntentId = "pi_original",
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
        context.Orders.Add(existingOrder);
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new OrderCreatedConsumer(context, logger);
        var @event = CreateEvent(orderId: "existing-order");

        var contextMock = new Mock<ConsumeContext<OrderCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var orders = await context.Orders.Where(o => o.Id == "existing-order").ToListAsync();
        orders.Should().HaveCount(1);
        orders[0].BuyerEmail.Should().Be("original@test.com");
    }

    [Fact]
    public async Task Consume_MapsAllFieldsCorrectly()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new OrderCreatedConsumer(context, logger);
        var @event = CreateEvent();

        var contextMock = new Mock<ConsumeContext<OrderCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == @event.OrderId);
        order.Should().NotBeNull();
        order!.BillingName.Should().Be("Test User");
        order.BillingLine1.Should().Be("123 Test St");
        order.BillingCity.Should().Be("Test City");
        order.BillingState.Should().Be("TS");
        order.BillingPostalCode.Should().Be("12345");
        order.BillingCountry.Should().Be("US");
        order.PaymentLast4.Should().Be(4242);
        order.PaymentBrand.Should().Be("Visa");
        order.PaymentIntentId.Should().Be("pi_test");
    }

    [Fact]
    public async Task Consume_SetsLastSyncedAt()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new OrderCreatedConsumer(context, logger);
        var @event = CreateEvent();

        var contextMock = new Mock<ConsumeContext<OrderCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == @event.OrderId);
        order!.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
