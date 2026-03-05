using Domain.Entities;

namespace Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<Domain.Entities.Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    IQueryable<Domain.Entities.Product> GetQueryable();
    void Add(Domain.Entities.Product product);
    void Update(Domain.Entities.Product product);
    void Remove(Domain.Entities.Product product);
}
