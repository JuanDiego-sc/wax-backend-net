using Application.Interfaces.Repositories.WriteRepositories;
using Domain.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.WriteRepositories;
public class ProductRepository(WriteDbContext context) : IProductRepository
{
    public async Task<CatalogProduct?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await context.Products.OfType<CatalogProduct>()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public IQueryable<CatalogProduct> GetQueryable()
    {
        return context.Products.OfType<CatalogProduct>().AsQueryable();
    }

    public void Add(CatalogProduct product)
    {
        context.Products.Add(product);
    }

    public void Remove(CatalogProduct product)
    {
        context.Products.Remove(product);
    }
}
