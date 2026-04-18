using Application.Core;
using Application.Core.Validations;
using Application.IntegrationEvents.ProductEvents;
using Application.Interfaces;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using MediatR;

namespace Application.Product.Commands;

public class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IImageService imageService,
    IEventPublisher eventPublisher)
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
        
        await eventPublisher.PublishEventAsync(new ProductDeletedIntegrationEvent
        {
            ProductId = request.ProductId,
        }, cancellationToken);

        productRepository.Remove(product);

        var result = await unitOfWork.CompleteAsync(cancellationToken);
        return !result 
            ? Result<Unit>.Failure("Failed to delete product")
            : Result<Unit>.Success(Unit.Value);
    }
}
