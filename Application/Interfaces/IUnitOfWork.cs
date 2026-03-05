namespace Application.Interfaces;

public interface IUnitOfWork
{
    Task<bool> CompleteAsync(CancellationToken cancellationToken = default);
    bool HasChanges();
}
