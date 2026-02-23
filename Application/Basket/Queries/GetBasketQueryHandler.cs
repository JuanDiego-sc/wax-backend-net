using System;
using Application.Basket.DTOs;
using Application.Basket.Extensions;
using Application.Core;
using MediatR;
using Persistence;

namespace Application.Basket.Queries;

public class GetBasketQueryHandler(AppDbContext context) : IRequestHandler<GetBasketQuery, Result<BasketDto>>
{
    public async Task<Result<BasketDto>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basket = await context.Baskets.GetBasketWithItems(request.BasketId);
        
        if (basket == null) return Result<BasketDto>.Failure("Basket not found");
        
        return Result<BasketDto>.Success(basket.ToDto());
    }
}
