using System;
using Application.Core;
using Application.Core.Pagination;
using Application.Interfaces;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using Domain.OrderAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Orders.Queries;

public class GetOrdersQueryHandler(AppDbContext context) : 
    IRequestHandler<GetOrdersQuery, Result<InfinityPagedList<OrderDto, DateTime?>>>
{
    public async Task<Result<InfinityPagedList<OrderDto, DateTime?>>> Handle(
        GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = context.Orders
            .AsNoTracking()
            .OrderBy(x => x.CreatedAt)
            .Where(x => x.CreatedAt >= (request.OrderParams.Cursor ?? request.OrderParams.StartDate))
            .AsQueryable();
        
        if(!string.IsNullOrEmpty(request.OrderParams.Filter))
        {
            query = request.OrderParams.Filter.ToLower() switch
            {
                "pending" => query.Where(x => x.OrderStatus == OrderStatus.Pending),
                "completed" => query.Where(x => x.OrderStatus == OrderStatus.Approved),
                "cancelled" => query.Where(x => x.OrderStatus == OrderStatus.Rejected),
                "paymentFailed" => query.Where(x => x.OrderStatus == OrderStatus.PaymentFailed),
                "paymentRecieved" => query.Where(x => x.OrderStatus == OrderStatus.PaymentRecieved),
                "paymentMismatch" => query.Where(x => x.OrderStatus == OrderStatus.PaymentMismatch),
                "customOrder" => query.Where(x => x.OrderStatus == OrderStatus.CustomOrder),
                _ => query
            };
        }

        var projectedOrders = query.ProjectToDto();

        var orders = await projectedOrders
        .Take(request.OrderParams.PageSize + 1)
        .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (orders.Count > request.OrderParams.PageSize){
            nextCursor = orders.Last().CreateAt;
            orders.RemoveAt(orders.Count - 1);
        }

        return Result<InfinityPagedList<OrderDto, DateTime?>>.Success(
            new InfinityPagedList<OrderDto, DateTime?>{
                Items = orders,
                NextCursor = nextCursor 
            }
        );
    }
}
