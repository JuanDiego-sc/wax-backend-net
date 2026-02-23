using System;
using Application.Core;
using Application.Product.DTOs;
using Application.Product.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Product.Queries;

public class GetProductDetailsQueryHandler(AppDbContext context) : IRequestHandler<GetProductDetailsQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null) return Result<ProductDto>.Failure("Product not found");

        return Result<ProductDto>.Success(product.ToDto());
    }
}
