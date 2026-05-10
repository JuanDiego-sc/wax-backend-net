using Application.Core.Validations;
using Application.CustomProducts.DTOs;
using Application.CustomProducts.Extensions;
using Application.IntegrationEvents.CustomProductEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Interfaces.Services;
using MediatR;

namespace Application.CustomProducts.Commands;

public class SubmitCustomProductCommandHandler(
    ICustomProductRepository customProductRepository,
    IQuotationService quotationService,
    IEventPublisher eventPublisher,
    IUnitOfWork unitOfWork) 
    : IRequestHandler<SubmitCustomProductCommand, Result<CustomProductDto>>
{
    public async Task<Result<CustomProductDto>> Handle(SubmitCustomProductCommand request, CancellationToken cancellationToken)
    {
        var existingCustomProduct =
            await customProductRepository.GetByTaskIdAsync(request.CustomProduct.TaskId, cancellationToken);
        if (existingCustomProduct != null)
            return Result<CustomProductDto>.Failure("Already exists a custom product with that TaskId");

        var product = request.CustomProduct.ToEntity(request.OwnerUserId);
        product.BasketId = request.BasketId;

        var quotation = await quotationService.QuoteAsync(product.Design, cancellationToken);
        product.RegisterSystemQuotation(quotation.Amount);
        
        customProductRepository.Add(product);
        
        await eventPublisher.PublishEventAsync(new CustomProductSubmittedIntegrationEvent
        {
            CustomProductId = product.Id,
            TaskId = product.TaskId,
            OwnerUserId = product.OwnerUserId,
            Name = product.Name,
            Description = product.Description,
            QuotedPrice = quotation.Amount,
            GlbUrl = product.GlbUrl,
            DesignType = product.Design.Type,
            DesignMaterial = product.Design.Material,
            DesignColor = product.Design.Color,
            DesignShape = product.Design.Shape,
            DesignDimensions = product.Design.Dimensions,
            DesignDetails = product.Design.Details
        }, cancellationToken);
        
        var result = await unitOfWork.CompleteAsync(cancellationToken);
        
        return result 
            ? Result<CustomProductDto>.Success(product.ToDto())
            : Result<CustomProductDto>.Failure("An error occured while submitting the custom product");
        
    }
}