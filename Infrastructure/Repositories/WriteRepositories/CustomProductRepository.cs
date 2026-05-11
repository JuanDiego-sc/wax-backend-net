using Application.Interfaces.Repositories.WriteRepositories;
using Domain.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.WriteRepositories;

public class CustomProductRepository(WriteDbContext context) : ICustomProductRepository
{
    public Task<CustomProduct?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        => context.CustomProducts.Include(p => p.Proposals).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<CustomProduct?> GetByTaskIdAsync(string taskId, CancellationToken cancellationToken = default)
        => context.CustomProducts.Include(p => p.Proposals).FirstOrDefaultAsync(p => p.TaskId == taskId, cancellationToken);

    public void Add(CustomProduct product) => context.CustomProducts.Add(product);

    public void Remove(CustomProduct product) => context.CustomProducts.Remove(product);
}