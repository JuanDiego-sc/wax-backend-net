using System;
using Application.Core;
using Application.Core.Pagination;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using MediatR;

namespace Application.Orders.Queries;

public class GetOrdersQuery : IRequest<Result<InfinityPagedList<OrderDto, DateTime?>>>
{
    public required OrderParams OrderParams { get; set; }
}
