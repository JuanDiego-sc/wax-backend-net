using Application.Payment.DTOs;
using Application.Payment.Events;
using DomainBasket = Domain.Entities.Basket;

namespace Application.Interfaces.Services;

public interface IPaymentService
{   
    Task<PaymentIntentResult> CreateOrUpdatePaymentIntent(DomainBasket basket);
    StripeEventResult ConstructStripeEvent(string payload, string signature);
}
