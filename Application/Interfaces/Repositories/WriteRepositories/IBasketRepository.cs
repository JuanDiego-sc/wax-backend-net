namespace Application.Interfaces.Repositories.WriteRepositories;

public interface IBasketRepository
{
    Task<Domain.Entities.Basket?> GetBasketWithItemsAsync(string? basketId, CancellationToken cancellationToken = default);
    void Add(Domain.Entities.Basket basket);
    void Remove(Domain.Entities.Basket basket);
}
