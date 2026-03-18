using Application.Core;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WriteRepositores;
using MediatR;

namespace Application.Product.Commands;

public class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IImageService imageService)
    : IRequestHandler<DeleteProductCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null) return Result<Unit>.Failure("Product not found");

        if (!string.IsNullOrEmpty(product.PublicId))
        {
            await imageService.DeleteImage(product.PublicId);
        }

        productRepository.Remove(product);

        var result = await unitOfWork.CompleteAsync(cancellationToken);
        return !result 
            ? Result<Unit>.Failure("Failed to delete product")
            : Result<Unit>.Success(Unit.Value);
    }
}
