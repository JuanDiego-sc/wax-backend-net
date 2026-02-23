using Application.Basket.DTOs;
using Microsoft.EntityFrameworkCore;
using DomainBasket = Domain.Entities.Basket;

namespace Application.Basket.Extensions;

public static class BasketExtensions
{
    public static BasketDto ToDto(this DomainBasket basket)
    {
        return new BasketDto
            {
                BasketId = basket.BasketId,
                ClientSecret = basket.ClientSecret,
                PaymentIntentId = basket.PaymentIntentId,
                Items = [.. basket.Items.Select(x => new BasketItemDto
                {
                    ProductId = x.ProductId,
                    ProductName = x.Product.Name,
                    Price = x.Product.Price,
                    PictureUrl = x.Product.PictureUrl,
                    Brand = x.Product.Brand,
                    Type = x.Product.Type,
                    Quantity = x.Quantity
                })]
            };
    }

    public static async Task<DomainBasket?> GetBasketWithItems(this IQueryable<DomainBasket> query, 
        string? basketId)
    {
        if (string.IsNullOrEmpty(basketId)) return null;
        
        return await query
                .AsNoTracking()  
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)    
                .FirstOrDefaultAsync(x => x.BasketId == basketId);
    }
}