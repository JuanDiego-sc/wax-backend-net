using Application.IntegrationEvents.CustomProductEvents;
using Application.Interfaces.Repositories.WriteRepositories;
using Domain.Entities;
using Domain.ProductAggregate;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Infrastructure.Messaging.Consumers.CustomProductConsumers;

public class CustomProductPriceAgreedConsumer(
    IBasketRepository basketRepository,
    ICustomProductRepository customProductRepository,
    IUnitOfWork unitOfWork,
    ReadDbContext readContext,
    ILogger<CustomProductPriceAgreedConsumer> logger)
    : IConsumer<CustomProductPriceAgreedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<CustomProductPriceAgreedIntegrationEvent> consumeContext)
    {
        var message = consumeContext.Message;
        var product = await customProductRepository.GetByIdAsync(message.CustomProductId, consumeContext.CancellationToken);
        if (product == null)
        {
            logger.LogWarning("CustomProduct {Id} not found while consume CustomProductPriceAgreed", message.CustomProductId);
            return;
        }
        if (product.Status == CustomProductStatus.AddedToBasket)
        {
            logger.LogInformation("CustomProduct {Id} already exists", message.CustomProductId);
            return;
        }

        var basket = await basketRepository.GetBasketWithItemsAsync(message.BasketId, consumeContext.CancellationToken);
        if (basket == null)
        {
            basket = new Basket { BasketId = message.BasketId };
            basketRepository.Add(basket);
            logger.LogInformation("Basket {BasketId} did not exist; created on-the-fly for CustomProduct {Id}",
                message.BasketId, message.CustomProductId);
        }

        basket.AddItem(product, 1);
        product.MarkAddedToBasket();

        await unitOfWork.CompleteAsync(consumeContext.CancellationToken);

        var readModel = await readContext.CustomProducts
            .FirstOrDefaultAsync(p => p.Id == product.Id, consumeContext.CancellationToken);
        if (readModel is not null)
        {
            readModel.Status = CustomProductStatus.AddedToBasket.ToString();
            readModel.AgreedPrice = message.AgreedPrice;
            readModel.Price = message.AgreedPrice;
            readModel.UpdatedAt = DateTime.UtcNow;
            readContext.Entry(readModel).State = EntityState.Modified;
            await readContext.SaveChangesAsync(consumeContext.CancellationToken);
        }

        logger.LogInformation("CustomProduct {Id} added to the cart successfully {BasketId}", product.Id, message.BasketId);
    }
}
