using Application.Basket.DTOs;
using Domain.ProductAggregate;
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
                    Kind = x.Product.Kind,
                    Brand = (x.Product as CatalogProduct)?.Brand,                                                                                                    
                    Type = (x.Product as CatalogProduct)?.Type,                                                                                                      
                    GlbUrl = (x.Product as CustomProduct)?.GlbUrl,
                    Quantity = x.Quantity 
                })]
            };
    }

    public static async Task<DomainBasket?> GetBasketWithItems(this IQueryable<DomainBasket> query, 
        string? basketId)
    {
        if (string.IsNullOrEmpty(basketId)) return null;
        
        return await query
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)    
                .FirstOrDefaultAsync(x => x.BasketId == basketId);
    }
}