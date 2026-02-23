using System;
using Application.Basket.DTOs;
using Application.Basket.Extensions;
using Application.Core;
using MediatR;
using Microsoft.AspNetCore.Http;
using Persistence;
using DomainBasket = Domain.Entities.Basket;

namespace Application.Basket.Commands;

public class AddItemCommandHandler(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<AddItemCommand, Result<BasketDto>>
{
    public async Task<Result<BasketDto>> Handle(AddItemCommand request, CancellationToken cancellationToken)
    {
        var basket = await context.Baskets.GetBasketWithItems(request.BasketId);

        basket ??= CreateBasket();

        var product = await context.Products.FindAsync(request.ProductId);

        if (product == null) return Result<BasketDto>.Failure("Product not found");

        basket.AddItem(product, request.Quantity);

        var result = await context.SaveChangesAsync(cancellationToken) > 0;
        if (!result) return Result<BasketDto>.Failure("Failed to add item to basket");

        return Result<BasketDto>.Success(basket.ToDto());

    }

    #region Private Methods
    private DomainBasket CreateBasket()
    {
        var basketId = Guid.NewGuid().ToString();
        var cookieOptions = new CookieOptions
        {
            IsEssential = true,
            Expires = DateTime.UtcNow.AddDays(30)
        };

        httpContextAccessor.HttpContext?.Response.Cookies.Append("basketId", basketId, cookieOptions);

        var basket = new DomainBasket { BasketId = basketId };

        context.Baskets.Add(basket);
        return basket;

    }
    #endregion
}
