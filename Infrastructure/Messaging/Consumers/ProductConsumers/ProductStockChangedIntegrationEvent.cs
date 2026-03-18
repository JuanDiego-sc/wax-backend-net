using Application.IntegrationEvents.ProductEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Messaging.Consumers.ProductConsumers;

public class ProductStockChangedConsumer(ReadDbContext readContext) :
    IConsumer<ProductStockChangedIntegrationEvent>

{
    public async Task Consume(ConsumeContext<ProductStockChangedIntegrationEvent> context)
    {
        var message = context.Message;
        
        var readModel = await readContext.Products
            .FirstOrDefaultAsync(p => p.Id == message.ProductId, context.CancellationToken );
        
        if (readModel is null) throw new InvalidOperationException("Product read model not found");

        readModel.QuantityInStock = message.NewQuantity;
        readModel.UpdatedAt = message.OccurredAt;
        readModel.LastSyncedAt = DateTime.UtcNow;
        
        readContext.Entry(readModel).State = EntityState.Modified;
        await readContext.SaveChangesAsync(context.CancellationToken);
    }
}