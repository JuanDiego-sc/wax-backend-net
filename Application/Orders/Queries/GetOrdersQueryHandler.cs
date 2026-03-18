using Application.Core;
using Application.Core.Pagination;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WriteRepositores;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using Domain.OrderAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries;

public class GetOrdersQueryHandler(IOrderRepository orderRepository) :
    IRequestHandler<GetOrdersQuery, Result<InfinityPagedList<OrderDto, DateTime?>>>
{
    public async Task<Result<InfinityPagedList<OrderDto, DateTime?>>> Handle(
        GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = orderRepository.GetQueryable()
            .AsNoTracking()
            .OrderBy(x => x.CreatedAt)
            .Where(x => x.CreatedAt >= (request.OrderParams.Cursor ?? request.OrderParams.StartDate))
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.OrderParams.Filter))
        {
            query = request.OrderParams.Filter.ToLower() switch
            {
                "pending" => query.Where(x => x.OrderStatus == OrderStatus.Pending),
                "completed" => query.Where(x => x.OrderStatus == OrderStatus.Approved),
                "cancelled" => query.Where(x => x.OrderStatus == OrderStatus.Rejected),
                "paymentfailed" => query.Where(x => x.OrderStatus == OrderStatus.PaymentFailed),
                "paymentrecieved" => query.Where(x => x.OrderStatus == OrderStatus.PaymentRecieved),
                "paymentmismatch" => query.Where(x => x.OrderStatus == OrderStatus.PaymentMismatch),
                "customorder" => query.Where(x => x.OrderStatus == OrderStatus.CustomOrder),
                _ => query
            };
        }

        var projectedOrders = query.ProjectToDto();

        var orders = await projectedOrders
            .Take(request.OrderParams.PageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (orders.Count > request.OrderParams.PageSize)
        {
            nextCursor = orders.Last().CreateAt;
            orders.RemoveAt(orders.Count - 1);
        }

        return Result<InfinityPagedList<OrderDto, DateTime?>>.Success(
            new InfinityPagedList<OrderDto, DateTime?>
            {
                Items = orders,
                NextCursor = nextCursor
            }
        );
    }
}
