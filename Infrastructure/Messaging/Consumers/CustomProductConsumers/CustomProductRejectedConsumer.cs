using Application.IntegrationEvents.CustomProductEvents;
using Domain.ProductAggregate;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.Messaging.Consumers.CustomProductConsumers;

public class CustomProductRejectedConsumer(
    ReadDbContext readContext,
    ILogger<CustomProductRejectedConsumer> logger)
    : IConsumer<CustomProductRejectedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CustomProductRejectedIntegrationEvent> context)
    {
        var message = context.Message;

        var readModel = await readContext.CustomProducts
            .FirstOrDefaultAsync(p => p.Id == message.CustomProductId, context.CancellationToken);
        if (readModel is null)
        {
            logger.LogWarning("CustomProduct {Id} not found in read model on Rejected", message.CustomProductId);
            return;
        }

        readModel.Status = CustomProductStatus.Rejected.ToString();
        readModel.UpdatedAt = message.OccurredAt;

        await readContext.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("CustomProduct {Id} marked as rejected in read model", message.CustomProductId);
    }
}
