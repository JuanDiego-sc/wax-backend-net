using Application.Basket.Interfaces;
using Application.Core.Pagination;
using Application.Orders.Commands;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using Application.Orders.Queries;
using Domain.Enumerators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class OrderController(IBasketProvider basketProvider) : BaseApiController
{
    [Authorize(Roles = Roles.Registered)]
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    [ProducesResponseType(typeof(List<OrderDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<PagedList<OrderDto>>> GetOrders([FromQuery] OrderParams orderParams)
    {
        return await HandlePagedQuery(new GetOrdersQuery { OrderParams = orderParams });
    }

    [Authorize(Roles = Roles.Registered)]
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<OrderDto>> GetOrderDetails(string id)
    {
        return await HandleQuery(new GetOrderDetailsQuery { OrderId = id });
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto orderDto)
    {
        var basketId = basketProvider.GetBasketId() ?? string.Empty;
        return await HandleCommand(new CreateOrderCommand { OrderDto = orderDto, BasketId = basketId });
    }
}
