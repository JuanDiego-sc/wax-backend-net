using Domain.ProductAggregate;

namespace Application.Interfaces.Repositories.WriteRepositories;

public interface IProductRepository
{
    Task<CatalogProduct?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    IQueryable<CatalogProduct> GetQueryable();
    void Add(CatalogProduct product);
    void Remove(CatalogProduct product);
}
