using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Infrastructure.Payments;

public class PaymentService(IConfiguration configuration) : IPaymentService
{
    public async Task<PaymentIntentResult> CreateOrUpdatePaymentIntent(Basket basket)
    {
        StripeConfiguration.ApiKey = configuration["StripeSettings:SecretKey"];

        var service = new PaymentIntentService();

        var intent = new PaymentIntent();
        var subtotal = basket.Items.Sum(item => item.Quantity * item.Product.Price);
        //var deliveryFee = subtotal > 10000 ? 0 : 500;

        if(string.IsNullOrEmpty(basket.PaymentIntentId))
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
}
