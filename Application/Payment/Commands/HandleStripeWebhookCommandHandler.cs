using Application.Core;
using Application.Interfaces;
using Application.Interfaces.Repositories.WriteRepositores;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Payment.Events;
using Domain.OrderAggregate;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Application.Payment.Commands;

public class HandleStripeWebhookCommandHandler(
    IPaymentService paymentService,
    IConfiguration configuration,
    IOrderRepository orderRepository,
    IBasketRepository basketRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    ILogger<HandleStripeWebhookCommandHandler> logger)
    : IRequestHandler<HandleStripeWebhookCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(HandleStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var webhookSecret = configuration["StripeSettings:WebhookSecret"] ?? "";
            var stripeEvent = paymentService.ConstructStripeEvent(request.Payload, request.Signature, webhookSecret);

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
            ?? throw new Exception("Order not found");

        foreach (var item in order.OrderItems)
        {
            var productItem = await productRepository.GetByIdAsync(item.ItemOrdered.ProductId, cancellationToken)
                ?? throw new Exception("Problem updating order stock");

            productItem.QuantityInStock += item.Quantity;
        }

        order.OrderStatus = OrderStatus.PaymentFailed;

        await unitOfWork.CompleteAsync(cancellationToken);
    }

    private async Task HandlePaymentSucceeded(StripeEventResult stripeEvent, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByPaymentIntentIdAsync(stripeEvent.IntentId, cancellationToken)
            ?? throw new Exception("Order not found");

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

        await unitOfWork.CompleteAsync(cancellationToken);
    }
    #endregion
}
