using Application.IntegrationEvents.OrderEvents;
using Infrastructure.Messaging.Consumers.OrderConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers.OrderConsumers;

public class OrderStatusChangedConsumerTests
{
    private static ReadDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReadDbContext(options);
    }

    private static ILogger<OrderStatusChangedConsumer> CreateLogger()
    {
        var logger = new Mock<ILogger<OrderStatusChangedConsumer>>();
        return logger.Object;
    }

    private static OrderReadModel CreateOrderReadModel(string orderId, string status = "Pending") => new()
    {
        Id = orderId,
        BuyerEmail = "buyer@test.com",
        OrderStatus = status,
        Subtotal = 5000,
        DeliveryFee = 500,
        Total = 5500,
        BillingName = "Test User",
        BillingLine1 = "123 Test St",
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
        CreatedAt = DateTime.UtcNow,
        LastSyncedAt = DateTime.UtcNow.AddMinutes(-5)
    };

    [Fact]
    public async Task Consume_WhenOrderExists_UpdatesStatus()
    {
        using var context = CreateInMemoryContext();
        var orderId = Guid.NewGuid().ToString();
        context.Orders.Add(CreateOrderReadModel(orderId, "Pending"));
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new OrderStatusChangedConsumer(context, logger);
        var @event = new OrderStatusChangedIntegrationEvent
        {
            OrderId = orderId,
            NewStatus = "PaymentRecieved"
        };

        var contextMock = new Mock<ConsumeContext<OrderStatusChangedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        order.Should().NotBeNull();
        order!.OrderStatus.Should().Be("PaymentRecieved");
    }

    [Fact]
    public async Task Consume_WhenOrderNotFound_ThrowsException()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new OrderStatusChangedConsumer(context, logger);
        var @event = new OrderStatusChangedIntegrationEvent
        {
            OrderId = "non-existent",
            NewStatus = "Completed"
        };

        var contextMock = new Mock<ConsumeContext<OrderStatusChangedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(contextMock.Object);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Order read model not found");
    }

    [Fact]
    public async Task Consume_UpdatesUpdatedAtAndLastSyncedAt()
    {
        using var context = CreateInMemoryContext();
        var orderId = Guid.NewGuid().ToString();
        var originalOrder = CreateOrderReadModel(orderId);
        context.Orders.Add(originalOrder);
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new OrderStatusChangedConsumer(context, logger);
        var eventTime = DateTime.UtcNow;
        var @event = new OrderStatusChangedIntegrationEvent
        {
            OrderId = orderId,
            NewStatus = "Shipped",
            OccurredAt = eventTime
        };

        var contextMock = new Mock<ConsumeContext<OrderStatusChangedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        order!.UpdatedAt.Should().BeCloseTo(eventTime, TimeSpan.FromSeconds(1));
        order.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Consume_DoesNotModifyOtherFields()
    {
        using var context = CreateInMemoryContext();
        var orderId = Guid.NewGuid().ToString();
        context.Orders.Add(CreateOrderReadModel(orderId));
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new OrderStatusChangedConsumer(context, logger);
        var @event = new OrderStatusChangedIntegrationEvent
        {
            OrderId = orderId,
            NewStatus = "Delivered"
        };

        var contextMock = new Mock<ConsumeContext<OrderStatusChangedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
        order!.BuyerEmail.Should().Be("buyer@test.com");
        order.Subtotal.Should().Be(5000);
        order.DeliveryFee.Should().Be(500);
    }
}
