using Application.Core.Validations;
using Application.CustomProducts.DTOs;
using Application.Interfaces.Repositories.ReadRepositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.CustomProducts.Queries;

public class GetMyCustomProductsQueryHandler(ICustomProductReadRepository repository)
    : IRequestHandler<GetMyCustomProductsQuery, Result<List<CustomProductDto>>>
{
    public async Task<Result<List<CustomProductDto>>> Handle(GetMyCustomProductsQuery request, CancellationToken cancellationToken)
    {
        var items = await repository.GetByOwner(request.OwnerUserId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
        
        return Result<List<CustomProductDto>>.Success(items);
    }
}