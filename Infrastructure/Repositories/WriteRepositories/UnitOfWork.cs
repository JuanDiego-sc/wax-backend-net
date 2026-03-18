using Application.Interfaces;
using Application.Interfaces.Repositories.WriteRepositores;
using Persistence;

namespace Infrastructure.Repositories;

public class UnitOfWork(WriteDbContext context) : IUnitOfWork
{
    public async Task<bool> CompleteAsync(CancellationToken cancellationToken)
    {
        return await context.SaveChangesAsync(cancellationToken) > 0;
    }

    public bool HasChanges()
    {
        return context.ChangeTracker.HasChanges();
    }
}
