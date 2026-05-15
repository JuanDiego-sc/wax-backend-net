using Application.Core.Validations;
using Application.IntegrationEvents.ProductEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Product.Commands.Delete;
using MediatR;

namespace Application.Product.Commands;

public class DeleteProductCommandHandler(
    IProductRepository productRepository,
    IEnumerable<IProductDeletionStrategy> deletionStrategies,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher)
    : IRequestHandler<DeleteProductCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.FindAnyByIdAsync(request.ProductId, cancellationToken);
        if (product == null) return Result<Unit>.Failure("Product not found", 404);

        var strategy = deletionStrategies.FirstOrDefault(s => s.Kind == product.Kind);
        if (strategy == null)
            return Result<Unit>.Failure($"No deletion strategy registered for product kind '{product.Kind}'");

        var strategyResult = await strategy.ExecuteAsync(product, cancellationToken);
        if (!strategyResult.IsSuccess) return strategyResult;

        await eventPublisher.PublishEventAsync(new ProductDeletedIntegrationEvent
        {
            ProductId = request.ProductId,
        }, cancellationToken);

        var committed = await unitOfWork.CompleteAsync(cancellationToken);
        return !committed
            ? Result<Unit>.Failure("Failed to delete product")
            : Result<Unit>.Success(Unit.Value);
    }
}
