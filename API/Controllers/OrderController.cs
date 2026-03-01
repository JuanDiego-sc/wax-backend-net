using System;
using Application.Core.Pagination;
using Application.Orders.Commands;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using Application.Orders.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class OrderController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<InfinityPagedList<OrderDto, DateTime?>>> GetOrders([FromQuery] OrderParams orderParams)
    {
        var query = new GetOrdersQuery { OrderParams = orderParams };
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrderDetails(string id)
    {
        var query = new GetOrderDetailsQuery { OrderId = id };
        return HandleResult(await Mediator.Send(query));
    }

    [HttpPost]
    public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto orderDto)
    {
        var basketId = Request.Cookies["basketId"] ?? string.Empty;
        var command = new CreateOrderCommand { OrderDto = orderDto, BasketId = basketId };
        
        return HandleResult(await Mediator.Send(command));
    }
}
