using Application.Basket.Commands;
using Application.Basket.DTOs;
using Application.Basket.Interfaces;
using Application.Basket.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BasketController(IBasketProvider basketProvider) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<BasketDto>> GetBasket()
    {
        var basketId = basketProvider.GetBasketId() ?? string.Empty;
        return await HandleQuery(new GetBasketQuery { BasketId = basketId });
    }

    [HttpPost]
    public async Task<ActionResult<BasketDto>> AddItemToBasket(string productId, int quantity)
    {
        var basketId = basketProvider.GetBasketId() ?? string.Empty;
        var result = await HandleCommand(new AddItemCommand
            { 
                ProductId = productId, 
                Quantity = quantity, 
                BasketId = basketId 
            });

        if (result != null && string.IsNullOrWhiteSpace(basketId))
        {
            basketProvider.SetBasketId(basketId);
        }
        
        return result!;
    }

    [HttpDelete]
    public async Task<ActionResult<Unit>> RemoveItemFromBasket(string productId, int quantity)
    {
        var basketId = basketProvider.GetBasketId() ?? string.Empty;
        return await HandleCommand(new RemoveBasketItemCommand { ProductId = productId, Quantity = quantity, BasketId = basketId });
    }
}
