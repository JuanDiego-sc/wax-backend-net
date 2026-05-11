using Application.Core.Validations;
using Application.CustomProducts.DTOs;
using Application.Interfaces.Repositories.ReadRepositories;
using MediatR;

namespace Application.CustomProducts.Queries;

public class GetCustomProductsAdminQueryHandler(
    ICustomProductReadRepository repository) : IRequestHandler<GetCustomProductsAdminQuery, Result<List<CustomProductDto>>>
{
    public async Task<Result<List<CustomProductDto>>> Handle(GetCustomProductsAdminQuery request, CancellationToken cancellationToken)
    {
        var query = repository.GetQueryable();
        
        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(p => p.Status == request.Status);
        
        var items = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .ToListAsync(query.OrderByDescending(p => p.CreatedAt), cancellationToken);
        
        return Result<List<CustomProductDto>>.Success(items);
    }
}