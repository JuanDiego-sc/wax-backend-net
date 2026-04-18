using Application.Core;
using Application.Core.Pagination;
using Application.Core.Validations;
using Application.Interfaces.Repositories.ReadRepositories;
using Application.Orders.DTOs;
using Domain.OrderAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;


namespace Application.Orders.Queries;

public class GetOrdersQueryHandler(IOrderReadRepository orderRepository) :
    IRequestHandler<GetOrdersQuery, Result<InfinityPagedList<OrderDto, DateTime?>>>
{
    public async Task<Result<InfinityPagedList<OrderDto, DateTime?>>> Handle(
        GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = orderRepository.GetQueryable()
            .AsNoTracking()
            .OrderBy(x => x.CreateAt)
            .Where(x => x.CreateAt >= (request.OrderParams.Cursor ?? request.OrderParams.StartDate))
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.OrderParams.Filter))
        {
            query = request.OrderParams.Filter.ToLower() switch
            {
                "pending" => query.Where(x => x.OrderStatus == nameof(OrderStatus.Pending)),
                "completed" => query.Where(x => x.OrderStatus == nameof(OrderStatus.Approved)),
                "cancelled" => query.Where(x => x.OrderStatus == nameof(OrderStatus.Rejected)),
                "paymentfailed" => query.Where(x => x.OrderStatus == nameof(OrderStatus.PaymentFailed)),
                "paymentrecieved" => query.Where(x => x.OrderStatus == nameof(OrderStatus.PaymentRecieved)),
                "paymentmismatch" => query.Where(x => x.OrderStatus == nameof(OrderStatus.PaymentMismatch)),
                "customorder" => query.Where(x => x.OrderStatus == nameof(OrderStatus.CustomOrder)),
                _ => query
            };
        }

        var orders = await query
            .Take(request.OrderParams.PageSize + 1)
            .ToListAsync(cancellationToken);

        DateTime? nextCursor = null;
        if (orders.Count > request.OrderParams.PageSize)
        {
            nextCursor = orders[^1].CreateAt;
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
