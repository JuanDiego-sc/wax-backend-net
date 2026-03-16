using Application.Core;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Product.Commands;

public class UpdateProductCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork,
    IImageService imageService)
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
        
        var result = await unitOfWork.CompleteAsync(cancellationToken);
        return !result 
               ? Result<Unit>.Failure("Failed to update product")
               : Result<Unit>.Success(Unit.Value);
    }
}
