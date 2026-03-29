using Application.Interfaces.Repositories.WriteRepositories;
using Persistence;

namespace Infrastructure.Repositories.WriteRepositories;
public class UnitOfWork(WriteDbContext context) : IUnitOfWork
{
    public async Task<bool> CompleteAsync(CancellationToken cancellationToken = default)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }

    public bool HasChanges()
    {
        return context.ChangeTracker.HasChanges();
    }
}
