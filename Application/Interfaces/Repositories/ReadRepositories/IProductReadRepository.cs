using Application.Product.DTOs;

namespace Application.Interfaces.Repositories.ReadRepositories;

public interface IProductReadRepository
{
    Task<ProductDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    IQueryable<ProductDto> GetQueryable();
}