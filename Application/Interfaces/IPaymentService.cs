using Application.Payment.DTOs;
using DomainBasket = Domain.Entities.Basket;

namespace Application.Interfaces;

public interface IPaymentService
{   
    Task<PaymentIntentResult> CreateOrUpdatePaymentIntent(DomainBasket basket);
}
