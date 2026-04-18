using Application.Core;
using Application.Core.Validations;
using Application.Interfaces.Repositories.WriteRepositories;
using MediatR;

namespace Application.Basket.Commands;

public class RemoveBasketItemCommandHandler(
    IBasketRepository basketRepository,
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveBasketItemCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RemoveBasketItemCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetBasketWithItemsAsync(request.BasketId, cancellationToken);

        if (basket == null) return Result<Unit>.Failure("Basket not found");

        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null) return Result<Unit>.Failure("Product not found");

        basket.RemoveItem(product.Id, request.Quantity);

        var result = await unitOfWork.CompleteAsync(cancellationToken);
        if (!result) return Result<Unit>.Failure("Failed to remove item from basket");

        return Result<Unit>.Success(Unit.Value);
    }
}
