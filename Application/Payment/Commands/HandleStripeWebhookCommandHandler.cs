using Application.Core;
using Application.Core.Validations;
using Application.IntegrationEvents.OrderEvents;
using Application.IntegrationEvents.ProductEvents;
using Application.Interfaces;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Interfaces.Services;
using Application.Payment.Events;
using Domain.OrderAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Payment.Commands;

public class HandleStripeWebhookCommandHandler(
    IPaymentService paymentService,
    IOrderRepository orderRepository,
    IBasketRepository basketRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher,
    ILogger<HandleStripeWebhookCommandHandler> logger)
    : IRequestHandler<HandleStripeWebhookCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(HandleStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var stripeEvent = paymentService.ConstructStripeEvent(request.Payload, request.Signature);

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                await HandlePaymentSucceeded(stripeEvent, cancellationToken);
            }
            else
            {
                await HandlePaymentFailed(stripeEvent, cancellationToken);
            }

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling Stripe webhook");
            return Result<Unit>.Failure("Error handling Stripe webhook");
        }
    }

    #region Private Methods
    private async Task HandlePaymentFailed(StripeEventResult stripeEvent, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByPaymentIntentIdAsync(stripeEvent.IntentId, cancellationToken)
            ?? throw new InvalidOperationException("Order not found");

        foreach (var item in order.OrderItems)
        {
            var productItem = await productRepository.GetByIdAsync(item.ItemOrdered.ProductId, cancellationToken)
                ?? throw new InvalidOperationException("Problem updating order stock");

            productItem.QuantityInStock += item.Quantity;

            await eventPublisher.PublishEventAsync(new ProductStockChangedIntegrationEvent
            {
                ProductId = productItem.Id,
                NewQuantity = productItem.QuantityInStock
            }, cancellationToken);
        }

        order.OrderStatus = OrderStatus.PaymentFailed;
        
        await eventPublisher.PublishEventAsync(new OrderStatusChangedIntegrationEvent
        {
            OrderId = order.Id,
            NewStatus = order.OrderStatus.ToString()
        }, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);
    }

    private async Task HandlePaymentSucceeded(StripeEventResult stripeEvent, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByPaymentIntentIdAsync(stripeEvent.IntentId, cancellationToken)
            ?? throw new InvalidOperationException("Order not found");

        if (order.GetTotal() != stripeEvent.Amount)
        {
            order.OrderStatus = OrderStatus.PaymentMismatch;
        }
        else
        {
            order.OrderStatus = OrderStatus.PaymentRecieved;
        }

        var basket = await basketRepository.GetBasketWithItemsAsync(
            order.PaymentIntentId, cancellationToken);

        if (basket != null) basketRepository.Remove(basket);

        await eventPublisher.PublishEventAsync(new OrderStatusChangedIntegrationEvent
        {
            OrderId = order.Id,
            NewStatus = order.OrderStatus.ToString()
        }, cancellationToken);

        await unitOfWork.CompleteAsync(cancellationToken);
    }
    #endregion
}
