using Application.IntegrationEvents.OrderEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Messaging.Consumers.OrderConsumers;

public class OrderStatusChangedConsumer(ReadDbContext readContext) : IConsumer<OrderStatusChangedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<OrderStatusChangedIntegrationEvent> context)
    {
        var message = context.Message;

        var readModel = await readContext.Orders
            .FirstOrDefaultAsync(o => o.Id == message.OrderId, context.CancellationToken);

        if (readModel == null) throw new InvalidOperationException("Order read model not found");
            
        readModel.OrderStatus = message.NewStatus;
        readModel.UpdatedAt = message.OccurredAt;
        readModel.LastSyncedAt = DateTime.UtcNow;

        readContext.Entry(readModel).State = EntityState.Modified;
        await readContext.SaveChangesAsync(context.CancellationToken);
    }
}
