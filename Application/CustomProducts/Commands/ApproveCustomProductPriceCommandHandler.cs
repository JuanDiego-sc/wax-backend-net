using Application.Core.Validations;
using Application.CustomProducts.DTOs;
using Application.CustomProducts.Extensions;
using Application.IntegrationEvents.CustomProductEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using Domain.ProductAggregate;
using MediatR;

namespace Application.CustomProducts.Commands;

public class ApproveCustomProductPriceCommandHandler(
    ICustomProductRepository repository,
    IBasketRepository basketRepository,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork)
    : IRequestHandler<ApproveCustomProductPriceCommand, Result<CustomProductDto>>
{
    public async Task<Result<CustomProductDto>> Handle(ApproveCustomProductPriceCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.CustomProductId, cancellationToken);
        if (product == null) return Result<CustomProductDto>.Failure("CustomProduct not found", 404);

        var lastProposalSource = product.Proposals.LastOrDefault()?.Source;
        var adminApprovingCustomerOffer =
            request.Approver == ProposalSource.Admin && lastProposalSource == ProposalSource.Customer;

        if (request.Approver == ProposalSource.Customer)
        {
            var validationResult = await ValidateCustomerApprovalAsync(request, product, cancellationToken);
            if (validationResult != null) return validationResult;
        }

        product.Approve(request.Approver);

        var addToCart = request.Approver == ProposalSource.Customer || adminApprovingCustomerOffer;
        var publishResult = await PublishApprovalEventAsync(request, product, addToCart, cancellationToken);
        if (publishResult != null) return publishResult;

        var result = await unitOfWork.CompleteAsync(cancellationToken);
        return result
            ? Result<CustomProductDto>.Success(product.ToDto())
            : Result<CustomProductDto>.Failure("there was an error while approving");
    }

    private async Task<Result<CustomProductDto>?> ValidateCustomerApprovalAsync(
        ApproveCustomProductPriceCommand request,
        CustomProduct product,
        CancellationToken cancellationToken)
    {
        if (product.OwnerUserId != request.OwnerUserId)
            return Result<CustomProductDto>.Failure("The user does not match", 403);

        if (string.IsNullOrWhiteSpace(request.BasketId))
            return Result<CustomProductDto>.Failure("BasketId is required to approve");

        var basket = await basketRepository.GetBasketWithItemsAsync(request.BasketId, cancellationToken);
        if (basket == null) return Result<CustomProductDto>.Failure("Basket not found", 404);

        product.BasketId = request.BasketId;
        return null;
    }

    private async Task<Result<CustomProductDto>?> PublishApprovalEventAsync(
        ApproveCustomProductPriceCommand request,
        CustomProduct product,
        bool addToCart,
        CancellationToken cancellationToken)
    {
        if (addToCart)
        {
            var basketId = request.Approver == ProposalSource.Customer ? request.BasketId! : product.BasketId;

            if (string.IsNullOrWhiteSpace(basketId))
                return Result<CustomProductDto>.Failure("Customer basket is unknown; cannot complete approval");

            await eventPublisher.PublishEventAsync(new CustomProductPriceAgreedIntegrationEvent
            {
                CustomProductId = product.Id,
                OwnerUserId = product.OwnerUserId,
                BasketId = basketId,
                AgreedPrice = product.AgreedPrice ?? product.Price
            }, cancellationToken);
        }
        else
        {
            await eventPublisher.PublishEventAsync(new CustomProductPriceUpdatedIntegrationEvent
            {
                CustomProductId = product.Id,
                OwnerUserId = product.OwnerUserId,
                Status = product.Status.ToString(),
                Price = product.AgreedPrice ?? product.Price,
                ProposalId = product.Proposals[^1].Id,
                ProposalSource = product.Proposals[^1].Source.ToString()
            }, cancellationToken);
        }

        return null;
    }
}