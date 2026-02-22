using Application.DTOs;
using Domain.Entities;

namespace Application.Interfaces;

public interface IPaymentService
{   
    Task<PaymentIntentResult> CreateOrUpdatePaymentIntent(Basket basket);
}
