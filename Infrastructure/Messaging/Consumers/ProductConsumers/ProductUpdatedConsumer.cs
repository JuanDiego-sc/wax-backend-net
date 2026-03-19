using Application.IntegrationEvents.ProductEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace Infrastructure.Messaging.Consumers.ProductConsumers;

public class ProductUpdatedConsumer(ReadDbContext readContext, ILogger<ProductUpdatedConsumer> logger) : IConsumer<ProductUpdatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<ProductUpdatedIntegrationEvent> context)
    {
        var message = context.Message;

        var readModel = await readContext.Products
            .FirstOrDefaultAsync(p => p.Id == message.ProductId, context.CancellationToken);

        if (readModel == null)
        {
            logger.LogInformation($"Product with id {message.ProductId} not found, creating a new one");
            readContext.Products.Add(new ProductReadModel
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
            });
        }
        else
        {
            logger.LogInformation($"Product with id {message.ProductId} found, updating new one");
            readModel.Name = message.Name;
            readModel.Description = message.Description;
            readModel.Price = message.Price;
            readModel.PictureUrl = message.PictureUrl;
            readModel.Type = message.Type;
            readModel.Brand = message.Brand;
            readModel.QuantityInStock = message.QuantityInStock;
            readModel.PublicId = message.PublicId;
            readModel.UpdatedAt = message.OccurredAt;
            readModel.LastSyncedAt = DateTime.UtcNow;
            
            readContext.Entry(readModel).State = EntityState.Modified;
        }

        await readContext.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation($"Product with id {message.ProductId} updated successfully");
    }
}