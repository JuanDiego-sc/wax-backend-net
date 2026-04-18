using Application.Basket.DTOs;
using Application.Basket.Extensions;
using Application.Core;
using Application.Core.Validations;
using Application.Interfaces.Repositories.WriteRepositories;
using MediatR;
using DomainBasket = Domain.Entities.Basket;

namespace Application.Basket.Commands;

public class AddItemCommandHandler(
    IBasketRepository basketRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AddItemCommand, Result<BasketDto>>
{
    public async Task<Result<BasketDto>> Handle(AddItemCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetBasketWithItemsAsync(request.BasketId, cancellationToken);

        basket ??= CreateBasket();

        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null) return Result<BasketDto>.Failure("Product not found");

        basket.AddItem(product, request.Quantity);

        var result = await unitOfWork.CompleteAsync(cancellationToken);
        if (!result) return Result<BasketDto>.Failure("Failed to add item to basket");

        return Result<BasketDto>.Success(basket.ToDto());
    }

    #region Private Methods
    private DomainBasket CreateBasket()
    {
        var basketId = Guid.NewGuid().ToString();
        var basket = new DomainBasket { BasketId = basketId };

        basketRepository.Add(basket);
        return basket;
    }
    #endregion
}
