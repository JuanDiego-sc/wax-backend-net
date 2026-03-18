using Application.IntegrationEvents.OrderEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.ReadModels;

namespace Infrastructure.Messaging.Consumers.OrderConsumers;

public class OrderCreatedConsumer(ReadDbContext readContext) : IConsumer<OrderCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        var alreadyExists = await readContext.Orders
            .AnyAsync(o => o.Id == message.OrderId, context.CancellationToken);
            
        if (alreadyExists) return;

        var readModel = new OrderReadModel
        {
            Id = message.OrderId,
            BuyerEmail = message.BuyerEmail,
            OrderStatus = message.OrderStatus,
            Subtotal = message.Subtotal,
            DeliveryFee = message.DeliveryFee,
            Total = message.Total,
            CreatedAt = message.OccurredAt,
            BillingName = message.BillingName,
            BillingLine1 = message.BillingLine1,
            BillingLine2 = message.BillingLine2,
            BillingCity = message.BillingCity,
            BillingState = message.BillingState,
            BillingPostalCode = message.BillingPostalCode,
            BillingCountry = message.BillingCountry,
            PaymentLast4 = message.PaymentLast4,
            PaymentBrand = message.PaymentBrand,
            PaymentExpMonth = message.PaymentExpMonth,
            PaymentExpYear = message.PaymentExpYear,
            OrderItems = message.OrderItems,
            PaymentIntentId = message.PaymentIntentId,
            LastSyncedAt = DateTime.UtcNow
        };

        readContext.Orders.Add(readModel);
        await readContext.SaveChangesAsync(context.CancellationToken);
    }
}
