using Application.Payment.DTOs;
using Application.Payment.Events;
using DomainBasket = Domain.Entities.Basket;

namespace Application.Interfaces;

public interface IPaymentService
{   
    Task<PaymentIntentResult> CreateOrUpdatePaymentIntent(DomainBasket basket);
    StripeEventResult ConstructStripeEvent(string payload, string signature, string webHookSecret);
}
