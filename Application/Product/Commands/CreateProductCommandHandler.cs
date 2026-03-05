using System;
using Application.Core;
using Application.Interfaces;
using Application.Product.DTOs;
using MediatR;
using Persistence;

namespace Application.Product.Commands;

public class CreateProductCommandHandler(AppDbContext context, IImageService imageService) 
    : IRequestHandler<CreateProductCommand, Result<CreateProductDto>>
{
    public async Task<Result<CreateProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = request.ProductDto;

        if(product.File != null)
        {
            var imageResult = await imageService.UploadImage(product.File);
            if(imageResult == null) return Result<CreateProductDto>.Failure("Image upload failed");

            product.PictureUrl = imageResult.Url;
            product.PublicId = imageResult.PublicId;
        }

        context.Products.Add(product.ToEntity());

        var result = await context.SaveChangesAsync(cancellationToken) > 0;
        if(!result) return Result<CreateProductDto>.Failure("Failed to create product");
        
        return Result<CreateProductDto>.Success(product);
        
    }
}
