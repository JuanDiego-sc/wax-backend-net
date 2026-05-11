using Application.IntegrationEvents.CustomProductEvents;
using Domain.ProductAggregate;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace Infrastructure.Messaging.Consumers.CustomProductConsumers;

public class CustomProductSubmittedConsumer(
    ReadDbContext readContext,
    ILogger<CustomProductSubmittedConsumer> logger)
    : IConsumer<CustomProductSubmittedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CustomProductSubmittedIntegrationEvent> context)
    {
        var message = context.Message;

        var alreadyExists = await readContext.CustomProducts
            .AnyAsync(p => p.Id == message.CustomProductId, context.CancellationToken);
        if (alreadyExists)
        {
            logger.LogInformation("CustomProduct {Id} already exists in read model", message.CustomProductId);
            return;
        }

        var name = string.IsNullOrWhiteSpace(message.Name) ? message.DesignType : message.Name;

        var readModel = new CustomProductReadModel
        {
            Id = message.CustomProductId,
            Name = name,
            Description = message.Description,
            Price = message.QuotedPrice,
            PictureUrl = message.GlbUrl,
            TaskId = message.TaskId,
            GlbUrl = message.GlbUrl,
            OwnerUserId = message.OwnerUserId,
            Status = CustomProductStatus.AwaitingAdminReview.ToString(),
            DesignType = message.DesignType,
            DesignMaterial = message.DesignMaterial,
            DesignColor = message.DesignColor,
            DesignShape = message.DesignShape,
            DesignDimensions = message.DesignDimensions,
            DesignDetails = message.DesignDetails,
            CreatedAt = message.OccurredAt
        };

        readContext.CustomProducts.Add(readModel);
        await readContext.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("CustomProduct {Id} projected to read model", message.CustomProductId);
    }
}
