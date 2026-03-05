using System;
using Application.Core;
using Application.Interfaces;
using MediatR;
using Persistence;

namespace Application.Product.Commands;

public class DeleteProductCommandHandler(AppDbContext context, IImageService imageService) : IRequestHandler<DeleteProductCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Products.FindAsync(request.ProductId);
        if (product == null) return Result<Unit>.Failure("Product not found");

        if(!string.IsNullOrEmpty(product.PublicId))
        {
            await imageService.DeleteImage(product.PublicId);
        }

        context.Products.Remove(product);

        var result = await context.SaveChangesAsync(cancellationToken) > 0;
        if(!result) return Result<Unit>.Failure("Failed to delete product");

        return Result<Unit>.Success(Unit.Value);
    }
}
