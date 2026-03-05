using Application.Interfaces.Repositories;
using Application.Basket.Extensions;
using Persistence;

namespace Infrastructure.Repositories;

public class BasketRepository(AppDbContext context) : IBasketRepository
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
