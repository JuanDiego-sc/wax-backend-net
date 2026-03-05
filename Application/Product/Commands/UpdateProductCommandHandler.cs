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

        if (request.ProductDto.File != null)
        {
            if (!string.IsNullOrEmpty(product.PublicId))
            {
                await imageService.DeleteImage(product.PublicId);
            }

            var imageResult = await imageService.UploadImage(request.ProductDto.File);
            if (imageResult == null) return Result<Unit>.Failure("Image upload failed");

            product.PictureUrl = imageResult.Url;
            product.PublicId = imageResult.PublicId;
        }

        var updatedProduct = request.ProductDto.ApplyTo(product);
        productRepository.Update(updatedProduct);

        var result = await unitOfWork.CompleteAsync(cancellationToken);
        if (!result) return Result<Unit>.Failure("Failed to update product");

        return Result<Unit>.Success(Unit.Value);
    }
}
