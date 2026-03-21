using Application.IntegrationEvents.ProductEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.Messaging.Consumers.ProductConsumers;


public class ProductDeletedConsumer(ReadDbContext readContext, ILogger<ProductDeletedConsumer> logger) : IConsumer<ProductDeletedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductDeletedIntegrationEvent> context)
    {
        var readModel = await readContext.Products
            .FirstOrDefaultAsync(p => p.Id == context.Message.ProductId, context.CancellationToken);

        if (readModel != null)
        {
            readContext.Products.Remove(readModel);
            await readContext.SaveChangesAsync(context.CancellationToken);
            logger.LogInformation("Product with id {ProductId} has been deleted", context.Message.ProductId);
        }
    }
}