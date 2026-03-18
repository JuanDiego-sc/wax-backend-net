using Application.IntegrationEvents.ProductEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Messaging.Consumers;


public class ProductDeletedConsumer(ReadDbContext readContext) : IConsumer<ProductDeletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductDeletedIntegrationEvent> context)
    {
        var readModel = await readContext.Products
            .FirstOrDefaultAsync(p => p.Id == context.Message.ProductId, context.CancellationToken);

        if (readModel != null)
        {
            readContext.Products.Remove(readModel);
            await readContext.SaveChangesAsync(context.CancellationToken);
        }
    }
}