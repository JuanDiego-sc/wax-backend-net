using Application.IntegrationEvents.ProductEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace Infrastructure.Messaging.Consumers.ProductConsumers;

public class ProductCreatedConsumer(ReadDbContext readContext, ILogger<ProductCreatedConsumer> logger) 
    : IConsumer<ProductCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        var alreadyExists = await readContext.Products
            .AnyAsync(p => p.Id == message.ProductId, context.CancellationToken);
        if (alreadyExists)
        {
            logger.LogInformation("Product with id {MessageProductId} has already been added", message.ProductId);
            return;
        }
        
        var readModel = new ProductReadModel
        {
            Id = message.ProductId,
            Name = message.Name,
            Description = message.Description,
            Price = message.Price,
            PictureUrl = message.PictureUrl,
            Type = message.Type,
            Brand = message.Brand,
            QuantityInStock = message.QuantityInStock,
            PublicId = message.PublicId,
            CreatedAt = message.OccurredAt,
            LastSyncedAt = DateTime.UtcNow
        };
        
        readContext.Products.Add(readModel);
        await readContext.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("Product with id {MessageProductId} has been added", message.ProductId);
    }
}