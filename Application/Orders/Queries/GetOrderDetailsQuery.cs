using System;
using Application.Core;
using Application.Core.Pagination;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using MediatR;

namespace Application.Orders.Queries;

public class GetOrderDetailsQuery : IRequest<Result<OrderDto>>
{
    public required string OrderId { get; set; }
}
