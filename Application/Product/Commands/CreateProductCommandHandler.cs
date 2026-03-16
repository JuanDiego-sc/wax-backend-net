using Application.Core;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Product.DTOs;
using MediatR;

namespace Application.Product.Commands;

public class CreateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IImageService imageService)
    : IRequestHandler<CreateProductCommand, Result<CreateProductDto>>
{
    public async Task<Result<CreateProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = request.ProductDto;
        
        var imageResult = await imageService.UploadImage(request.ImageRequest, cancellationToken);
        if (imageResult == null) return Result<CreateProductDto>.Failure("Image upload failed");

        product.PictureUrl = imageResult.Url;
        product.PublicId = imageResult.PublicId;
        

        productRepository.Add(product.ToEntity());

        var result = await unitOfWork.CompleteAsync(cancellationToken);
        return !result 
            ? Result<CreateProductDto>.Failure("Failed to update product")
            : Result<CreateProductDto>.Success(product);
    }
}
