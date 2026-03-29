namespace Application.Interfaces.Repositories.WriteRepositories;

public interface IUnitOfWork
{
    Task<bool> CompleteAsync(CancellationToken cancellationToken = default);
    bool HasChanges();
}
