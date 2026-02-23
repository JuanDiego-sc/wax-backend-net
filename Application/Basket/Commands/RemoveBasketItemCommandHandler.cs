using System;
using Application.Basket.Extensions;
using Application.Core;
using MediatR;
using Persistence;

namespace Application.Basket.Commands;

public class RemoveBasketItemCommandHandler(AppDbContext context) : IRequestHandler<RemoveBasketItemCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RemoveBasketItemCommand request, CancellationToken cancellationToken)
    {
        var basket = context.Baskets.GetBasketWithItems(request.BasketId).Result;

        if (basket == null) return Result<Unit>.Failure("Basket not found");

        var product = await context.Products.FindAsync(request.ProductId);

        if (product == null) return Result<Unit>.Failure("Product not found");

        basket.RemoveItem(product.Id, request.Quantity);

        var result = await context.SaveChangesAsync(cancellationToken) > 0;
        if (!result) return Result<Unit>.Failure("Failed to remove item from basket");

        return Result<Unit>.Success(Unit.Value);
    }
}
