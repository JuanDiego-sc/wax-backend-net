using System;
using Application.Core;
using Application.Interfaces;
using Application.Payment.Events;
using Domain.OrderAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence;

namespace Application.Payment.Commands;

public class HandleStripeWebhookCommandHandler(IPaymentService paymentService, IConfiguration configuration, AppDbContext context,
    ILogger<HandleStripeWebhookCommandHandler> logger) : IRequestHandler<HandleStripeWebhookCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(HandleStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var webhookSecret = configuration["StripeSettings:WebhookSecret"] ?? "";
            var stripeEvent = paymentService.ConstructStripeEvent(request.Payload, request.Signature, webhookSecret);
            
            if(stripeEvent.Type == "succeeded")
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
        var order = await context.Orders
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.PaymentIntentId == stripeEvent.IntentId, cancellationToken: cancellationToken)
                ?? throw new Exception("Order not found");

        foreach(var item in order.OrderItems)
        {
            var productItem = await context.Products
                .FindAsync(new object?[] { item.ItemOrdered.ProductId }, cancellationToken: cancellationToken)
                    ?? throw new Exception("Problem updating order stock");

            productItem.QuantityInStock += item.Quantity;
        }

        order.OrderStatus = OrderStatus.PaymentFailed;

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task HandlePaymentSucceeded(StripeEventResult stripeEvent, CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.PaymentIntentId == stripeEvent.IntentId, cancellationToken: cancellationToken)
                ?? throw new Exception("Order not found");

        if(order.GetTotal() != stripeEvent.Amount)
        {
            order.OrderStatus = OrderStatus.PaymentMismatch;
        }
        else
        {
            order.OrderStatus = OrderStatus.PaymentRecieved;
        }

        var basket = await context.Baskets.FirstOrDefaultAsync(x => x.PaymentIntentId == stripeEvent.IntentId, cancellationToken: cancellationToken);

        if(basket != null) context.Baskets.Remove(basket);

        await context.SaveChangesAsync(cancellationToken);
    }
    
    #endregion
}
