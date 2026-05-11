using Application.Core.Validations;
using Application.IntegrationEvents.CustomProductEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using MediatR;

namespace Application.CustomProducts.Commands;

public class RejectCustomProductCommandHandler(
    ICustomProductRepository repository,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RejectCustomProductCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RejectCustomProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.CustomProductId, cancellationToken);
        if (product == null) return Result<bool>.Failure("Custom product not found", 404);

        product.Reject(request.Reason);

        await eventPublisher.PublishEventAsync(new CustomProductRejectedIntegrationEvent
        {
            CustomProductId = product.Id,
            OwnerUserId = product.OwnerUserId,
            Reason = request.Reason
        }, cancellationToken);

        var result = await unitOfWork.CompleteAsync(cancellationToken);
        return result ? Result<bool>.Success(true) : Result<bool>.Failure("there was an error while rejecting");
    }
}