using Application.Core;
using Application.Core.Pagination;
using Application.Core.Validations;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using MediatR;

namespace Application.Orders.Queries;

public record GetMyOrdersQuery(OrderParams Params)
    : IRequest<Result<PagedList<OrderDto>>>;
