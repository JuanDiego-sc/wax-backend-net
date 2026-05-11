using Application.Core.Validations;
using Application.CustomProducts.DTOs;
using Application.CustomProducts.Extensions;
using Application.Interfaces.Repositories.WriteRepositories;
using MediatR;

namespace Application.CustomProducts.Queries;

public class GetCustomProductDetailsQueryHandler(ICustomProductRepository repository)
    : IRequestHandler<GetCustomProductDetailsQuery, Result<CustomProductDto>>
{
    public async Task<Result<CustomProductDto>> Handle(GetCustomProductDetailsQuery request, CancellationToken cancellationToken)
    {
        var product = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (product == null) return Result<CustomProductDto>.Failure("Custom product not found", 404);
        
        if (!request.RequesterIsAdmin && product.OwnerUserId != request.RequesterUserId)
            return Result<CustomProductDto>.Failure("Access denied", 403);
        return Result<CustomProductDto>.Success(product.ToDto());
    }
}