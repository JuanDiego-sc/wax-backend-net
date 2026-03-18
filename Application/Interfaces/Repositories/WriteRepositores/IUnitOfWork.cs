namespace Application.Interfaces.Repositories.WriteRepositores;

public interface IUnitOfWork
{
    Task<bool> CompleteAsync(CancellationToken cancellationToken = default);
    bool HasChanges();
}
