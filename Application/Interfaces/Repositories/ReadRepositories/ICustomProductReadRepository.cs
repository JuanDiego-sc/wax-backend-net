using Application.CustomProducts.DTOs;

namespace Application.Interfaces.Repositories.ReadRepositories;

public interface ICustomProductReadRepository
{
    Task<CustomProductDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    IQueryable<CustomProductDto> GetQueryable();
    IQueryable<CustomProductDto> GetByOwner(string ownerUserId);
}
