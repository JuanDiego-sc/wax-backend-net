using Application.Basket.Commands;
using Application.Basket.DTOs;
using Application.Basket.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BasketController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<BasketDto>> GetBasket()
    {
        var basketId = Request.Cookies["basketId"] ?? string.Empty;
        return await HandleQuery(new GetBasketQuery { BasketId = basketId });
    }

    [HttpPost]
    public async Task<ActionResult<BasketDto>> AddItemToBasket(string productId, int quantity)
    {
        var basketId = Request.Cookies["basketId"] ?? string.Empty;
        return await HandleCommand(new AddItemCommand { ProductId = productId, Quantity = quantity, BasketId = basketId });
    }

    [HttpDelete]
    public async Task<ActionResult<Unit>> RemoveItemFromBasket(string productId, int quantity)
    {
        var basketId = Request.Cookies["basketId"] ?? string.Empty;
        return await HandleCommand(new RemoveBasketItemCommand { ProductId = productId, Quantity = quantity, BasketId = basketId });
    }
}
