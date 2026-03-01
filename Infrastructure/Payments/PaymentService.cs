using Application.Interfaces;
using Application.Payment.DTOs;
using Application.Payment.Events;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace Infrastructure.Payments;

public class PaymentService(IConfiguration configuration, ILogger<PaymentService> logger) : IPaymentService
{
    public async Task<PaymentIntentResult> CreateOrUpdatePaymentIntent(Basket basket)
    {
        StripeConfiguration.ApiKey = configuration["StripeSettings:SecretKey"];

        var service = new PaymentIntentService();

        var intent = new PaymentIntent();
        var subtotal = basket.Items.Sum(item => item.Quantity * item.Product.Price);
        var deliveryFee = subtotal > 10000 ? 0 : 500;

        if (string.IsNullOrEmpty(basket.PaymentIntentId))
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = subtotal,
                Currency = "usd",
                PaymentMethodTypes = ["card"]
            };

            intent = await service.CreateAsync(options);
            basket.PaymentIntentId ??= intent.Id;
            basket.ClientSecret ??= intent.ClientSecret;
        }
        else
        {
            var options = new PaymentIntentUpdateOptions
            {
                Amount = subtotal
            };

            await service.UpdateAsync(basket.PaymentIntentId, options);
        }

        return new PaymentIntentResult
        {
            PaymentIntentId = basket.PaymentIntentId,
            ClientSecret = basket.ClientSecret
        };
    }

    public StripeEventResult ConstructStripeEvent(string payload, string signature, string webHookSecret)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(payload, signature, webHookSecret);
            var (status, intentId, amount) = stripeEvent.Data.Object switch
            {
                PaymentIntent intent => (intent.Status, intent.Id, intent.Amount),
                _ => throw new StripeException("Unexpected event data object type")
            };

            return new StripeEventResult(stripeEvent.Type, status, intentId, amount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error constructing Stripe event");
            throw new StripeException("Invalid signature", ex);
        }
    }
}
