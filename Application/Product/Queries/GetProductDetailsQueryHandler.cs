using Application.Core;
using Application.Interfaces.Repositories;
using Application.Product.DTOs;
using Application.Product.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Product.Queries;

public class GetProductDetailsQueryHandler(IProductRepository productRepository) : IRequestHandler<GetProductDetailsQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null) return Result<ProductDto>.Failure("Product not found", 404);

        return Result<ProductDto>.Success(product.ToDto());
    }
}
