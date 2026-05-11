using Application.Core.Validations;
using Application.CustomProducts.DTOs;
using Application.CustomProducts.Extensions;
using Application.IntegrationEvents.CustomProductEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using MediatR;

namespace Application.CustomProducts.Commands;

public class CustomerProposePriceCommandHandler(
    ICustomProductRepository customProductRepository,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork) : IRequestHandler<CustomerProposePriceCommand, Result<CustomProductDto>>
{
    public async Task<Result<CustomProductDto>> Handle(CustomerProposePriceCommand request, CancellationToken cancellationToken)
    {
        var product = await customProductRepository.GetByIdAsync(request.CustomProductId, cancellationToken);
        if (product == null) return Result<CustomProductDto>.Failure("Custom product not found", 404);

        if (product.OwnerUserId != request.OwnerUserId)
            return Result<CustomProductDto>.Failure("The owner of the piece does not match", 403);

        if (!string.IsNullOrWhiteSpace(request.BasketId))
            product.BasketId = request.BasketId;

        var proposal =
            product.RegisterCustomerProposal(request.ProposeCustomPrice.Amount, request.ProposeCustomPrice.Comment);
        
        await eventPublisher.PublishEventAsync(new CustomProductPriceUpdatedIntegrationEvent
        {
            CustomProductId = product.Id,
            OwnerUserId = product.OwnerUserId,
            Status = product.Status.ToString(),
            Price = proposal.Amount,
            ProposalId = proposal.Id,
            ProposalSource = proposal.Source.ToString(),
            Comment = proposal.Comment
        }, cancellationToken);

        var result = await unitOfWork.CompleteAsync(cancellationToken);
        return result
            ? Result<CustomProductDto>.Success(product.ToDto())
            : Result<CustomProductDto>.Failure("can not save the proposal");
    }
}