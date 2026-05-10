using Application.IntegrationEvents.CustomProductEvents;
using Domain.ProductAggregate;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.Messaging.Consumers.CustomProductConsumers;

public class CustomProductPriceUpdatedConsumer(
    ReadDbContext readContext,
    ILogger<CustomProductPriceUpdatedConsumer> logger)
    : IConsumer<CustomProductPriceUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CustomProductPriceUpdatedIntegrationEvent> context)
    {
        var message = context.Message;

        var readModel = await readContext.CustomProducts
            .FirstOrDefaultAsync(p => p.Id == message.CustomProductId, context.CancellationToken);
        if (readModel is null)
        {
            logger.LogWarning("CustomProduct {Id} not found in read model on PriceUpdated", message.CustomProductId);
            return;
        }

        readModel.Price = message.Price;
        readModel.Status = message.Status;
        readModel.UpdatedAt = message.OccurredAt;

        if (message.Status == CustomProductStatus.Approved.ToString())
            readModel.AgreedPrice = message.Price;

        await readContext.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("CustomProduct {Id} price/status updated in read model", message.CustomProductId);
    }
}
