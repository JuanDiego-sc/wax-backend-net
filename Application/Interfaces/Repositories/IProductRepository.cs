using ProductDomain = Domain.Entities.Product;
namespace Application.Interfaces.Repositories;

public interface IProductRepository
{
    Task<ProductDomain?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    IQueryable<ProductDomain> GetQueryable();
    void Add(ProductDomain product);
    void Remove(ProductDomain product);
}
