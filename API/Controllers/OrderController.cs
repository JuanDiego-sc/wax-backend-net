using Application.Basket.Interfaces;
using Application.Core.Pagination;
using Application.Orders.Commands;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using Application.Orders.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class OrderController(IBasketProvider basketProvider) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<InfinityPagedList<OrderDto, DateTime?>>> GetOrders([FromQuery] OrderParams orderParams)
    {
        return await HandleQuery(new GetOrdersQuery { OrderParams = orderParams });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrderDetails(string id)
    {
        return await HandleQuery(new GetOrderDetailsQuery { OrderId = id });
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto orderDto)
    {
        var basketId = basketProvider.GetBasketId() ?? string.Empty;
        return await HandleCommand(new CreateOrderCommand { OrderDto = orderDto, BasketId = basketId });
    }
}
