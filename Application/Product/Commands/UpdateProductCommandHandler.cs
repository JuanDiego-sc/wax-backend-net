using Application.Core;
using Application.IntegrationEvents.ProductEvents;
using Application.Interfaces;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WriteRepositores;
using MediatR;

namespace Application.Product.Commands;

public class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IImageService imageService,
    IEventPublisher eventPublisher)
    : IRequestHandler<UpdateProductCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductDto.Id, cancellationToken);

        if (product == null) return Result<Unit>.Failure("Product not found");

        request.ProductDto.ApplyTo(product);

        if (request.ImageRequest?.Content != null)
        {
            if (!string.IsNullOrEmpty(product.PublicId))
            {
                await imageService.DeleteImage(product.PublicId, cancellationToken);
            }

            var imageResult = await imageService.UploadImage(request.ImageRequest, cancellationToken);
            if (imageResult == null) return Result<Unit>.Failure("Image upload failed");

            product.PictureUrl = imageResult.Url;
            product.PublicId = imageResult.PublicId;
        }
        
        await eventPublisher.PublishEventAsync(new ProductUpdatedIntegrationEvent
        {
            ProductId = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            PictureUrl = product.PictureUrl,
            Type = product.Type,
            Brand = product.Brand,
            QuantityInStock = product.QuantityInStock,
            PublicId = product.PublicId
        }, cancellationToken);
        
        var result = await unitOfWork.CompleteAsync(cancellationToken);
        return !result 
               ? Result<Unit>.Failure("Failed to update product")
               : Result<Unit>.Success(Unit.Value);
    }
}
