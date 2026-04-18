using Application.Core;
using Application.Core.Validations;
using Application.Interfaces.Repositories.ReadRepositories;
using Application.Product.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Product.Queries;

public class GetProductDetailsQueryHandler(IProductReadRepository productRepository) 
    : IRequestHandler<GetProductDetailsQuery, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(GetProductDetailsQuery request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetQueryable()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null) return Result<ProductDto>.Failure("Product not found", 404);

        return Result<ProductDto>.Success(product);
    }
}
