using Application.Basket.DTOs;
using Application.Basket.Extensions;
using Application.Core;
using Application.Interfaces.Repositories.WriteRepositores;
using MediatR;

namespace Application.Basket.Queries;

public class GetBasketQueryHandler(IBasketRepository basketRepository) 
    : IRequestHandler<GetBasketQuery, Result<BasketDto>>
{
    public async Task<Result<BasketDto>> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetBasketWithItemsAsync(request.BasketId, cancellationToken);

        if (basket == null) return Result<BasketDto>.Failure("Basket not found");

        return Result<BasketDto>.Success(basket.ToDto());
    }
}
