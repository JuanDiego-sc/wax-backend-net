using Application.Core.Validations;
using Application.CustomProducts.DTOs;
using Application.CustomProducts.Extensions;
using Application.IntegrationEvents.CustomProductEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using MediatR;

namespace Application.CustomProducts.Commands;

public class AdminProposePriceCommandHandler(
    ICustomProductRepository customProductRepository,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork) : IRequestHandler<AdminProposePriceCommand, Result<CustomProductDto>>
{
    public async Task<Result<CustomProductDto>> Handle(AdminProposePriceCommand request, CancellationToken cancellationToken)
    {
        var product = await customProductRepository.GetByIdAsync(request.CustomProductId, cancellationToken);
        if (product == null) return Result<CustomProductDto>.Failure("Custom Product not found", 404);

        var proposal =
            product.RegisterAdminProposal(request.ProposeCustomPrice.Amount, request.ProposeCustomPrice.Comment);
        
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
            : Result<CustomProductDto>.Failure("Proposal price not updated");
    }
}

