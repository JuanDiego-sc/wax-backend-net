using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WriteRepositores;
using Domain.Entities;
using Persistence;

namespace Infrastructure.Repositories.WriteRepositories;
public class ProductRepository(WriteDbContext context) : IProductRepository
{
    public async Task<Product?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await context.Products.FindAsync([id], cancellationToken);
    }

    public IQueryable<Product> GetQueryable()
    {
        return context.Products.AsQueryable();
    }

    public void Add(Product product)
    {
        context.Products.Add(product);
    }

    public void Remove(Product product)
    {
        context.Products.Remove(product);
    }
}
