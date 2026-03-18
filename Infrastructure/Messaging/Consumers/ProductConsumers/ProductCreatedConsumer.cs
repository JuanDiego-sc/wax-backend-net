using Application.IntegrationEvents.ProductEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.ReadModels;

namespace Infrastructure.Messaging.Consumers.ProductConsumers;

public class ProductCreatedConsumer(ReadDbContext readContext) : IConsumer<ProductCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        var alreadyExists = await readContext.Products
            .AnyAsync(p => p.Id == message.ProductId, context.CancellationToken);
        if (alreadyExists) return;
        
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
    }
}