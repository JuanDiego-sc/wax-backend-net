using System;
using Application.Basket.Commands;
using Application.Basket.DTOs;
using Application.Basket.Queries;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class BasketController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<BasketDto>> GetBasket()
    {
        var basketId = Request.Cookies["basketId"] ?? string.Empty;
        var query = new GetBasketQuery { BasketId = basketId };

        return HandleResult(await Mediator.Send(query));
    }

    [HttpPost]
    public async Task<ActionResult<BasketDto>> AddItemToBasket(string productId, int quantity)
    {
        var basketId = Request.Cookies["basketId"] ?? string.Empty;
        var command = new AddItemCommand {ProductId = productId, Quantity = quantity, BasketId = basketId};

        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete]
    public async Task<ActionResult<Unit>> RemoveItemFromBasket(string productId, int quantity)
    {
        var basketId = Request.Cookies["basketId"] ?? string.Empty;
        var command = new RemoveBasketItemCommand { ProductId = productId, Quantity = quantity, BasketId = basketId };
        return HandleResult(await Mediator.Send(command));
    }
}
