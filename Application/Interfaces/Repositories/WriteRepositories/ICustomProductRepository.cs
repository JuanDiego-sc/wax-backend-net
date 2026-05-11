using Domain.ProductAggregate;

namespace Application.Interfaces.Repositories.WriteRepositories;

public interface ICustomProductRepository
{
    Task<CustomProduct?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<CustomProduct?> GetByTaskIdAsync(string taskId, CancellationToken cancellationToken = default);
    void Add(CustomProduct product);
    void Remove(CustomProduct product);
}