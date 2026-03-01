using System;
using Application.Core;
using Application.Orders.DTOs;
using MediatR;

namespace Application.Orders.Commands;

public class CreateOrderCommand : IRequest<Result<OrderDto>>
{
    public required CreateOrderDto OrderDto { get; set; }
    public required string BasketId { get; set; }
}
