using System;
using Application.Basket.DTOs;
using Application.Basket.Extensions;
using Application.Core;
using Application.Interfaces;
using MediatR;
using Persistence;

namespace Application.Payment.Commands;

public class CreateOrUpdateIntentCommandHandler(AppDbContext context, IPaymentService paymentService) : IRequestHandler<CreateOrUpdateIntentCommand, Result<BasketDto>>
{
    public async Task<Result<BasketDto>> Handle(CreateOrUpdateIntentCommand request, CancellationToken cancellationToken)
    {
        var basket = await context.Baskets.GetBasketWithItems(request.BasketId);
        if(basket == null) return Result<BasketDto>.Failure("Basket not found");

        var intent = await paymentService.CreateOrUpdatePaymentIntent(basket);
        if (intent == null) return Result<BasketDto>.Failure("Problem creating payment intent");

        if (context.ChangeTracker.HasChanges())
        {
            var result = await context.SaveChangesAsync() > 0;
            if (!result) return Result<BasketDto>.Failure("Problem updating basket with intent");
        }

        return Result<BasketDto>.Success(basket.ToDto());
    }
}
