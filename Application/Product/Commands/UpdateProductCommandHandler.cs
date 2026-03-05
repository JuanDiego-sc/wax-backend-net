using System;
using Application.Core;
using Application.Interfaces;
using MediatR;
using Persistence;

namespace Application.Product.Commands;

public class UpdateProductCommandHandler(AppDbContext context, IImageService imageService) 
    : IRequestHandler<UpdateProductCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
       
        var product = await context.Products
        .FindAsync(request.ProductDto.Id);

        if (product == null) return Result<Unit>.Failure("Product not found");

        if(request.ProductDto.File != null)
        {
            if(!string.IsNullOrEmpty(product.PublicId))
            {
                await imageService.DeleteImage(product.PublicId);
            }

            var imageResult = await imageService.UploadImage(request.ProductDto.File);
            if(imageResult == null) return Result<Unit>.Failure("Image upload failed");

            product.PictureUrl = imageResult.Url;
            product.PublicId = imageResult.PublicId;
        }
        
        var updatedProduct = request.ProductDto.ApplyTo(product);
        context.Products.Update(updatedProduct);

        var result = await context.SaveChangesAsync(cancellationToken) > 0;
        if(!result) return Result<Unit>.Failure("Failed to update product");

        return Result<Unit>.Success(Unit.Value);
        
    }
}
