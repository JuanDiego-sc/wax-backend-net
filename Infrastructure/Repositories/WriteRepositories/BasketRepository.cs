using Application.Basket.Extensions;
using Application.Interfaces.Repositories.WriteRepositories;
using Persistence;

namespace Infrastructure.Repositories.WriteRepositories;
public class BasketRepository(WriteDbContext context) : IBasketRepository
{
    public async Task<Domain.Entities.Basket?> GetBasketWithItemsAsync(string? basketId, CancellationToken cancellationToken = default)
    {
        return await context.Baskets.GetBasketWithItems(basketId);
    }

    public void Add(Domain.Entities.Basket basket)
    {
        context.Baskets.Add(basket);
    }

    public void Remove(Domain.Entities.Basket basket)
    {
        context.Baskets.Remove(basket);
    }
}
