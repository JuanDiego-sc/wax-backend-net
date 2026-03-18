using Application.Core;
using Application.IntegrationEvents.ProductEvents;
using Application.Interfaces;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WriteRepositores;
using Application.Product.DTOs;
using Application.Product.Extensions;
using MediatR;

namespace Application.Product.Commands;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IImageService imageService,
    IEventPublisher eventPublisher)
    : IRequestHandler<CreateProductCommand, Result<CreateProductDto>>
{
    public async Task<Result<CreateProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = request.ProductDto.ToEntity();
        
        var imageResult = await imageService.UploadImage(request.ImageRequest, cancellationToken);
        if (imageResult == null) return Result<CreateProductDto>.Failure("Image upload failed");

        product.PictureUrl = imageResult.Url;
        product.PublicId = imageResult.PublicId;
        

        productRepository.Add(product);
        
        await eventPublisher.PublishEventAsync(new ProductCreatedIntegrationEvent
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
            ? Result<CreateProductDto>.Failure("Failed to update product")
            : Result<CreateProductDto>.Success(request.ProductDto);
    }
}
