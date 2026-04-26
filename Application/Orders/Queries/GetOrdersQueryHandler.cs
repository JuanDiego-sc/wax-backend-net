using Application.Core;
using Application.Core.Pagination;
using Application.Core.Validations;
using Application.Interfaces.Repositories.ReadRepositories;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using Domain.OrderAggregate;
using MediatR;

namespace Application.Orders.Queries;

public class GetOrdersQueryHandler(IOrderReadRepository orderRepository) :
    IRequestHandler<GetOrdersQuery, Result<PagedList<OrderDto>>>
{
    public async Task<Result<PagedList<OrderDto>>> Handle(
        GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var statusFilter = request.OrderParams.Filter?.ToLower() switch
        {
            "pending"         => nameof(OrderStatus.Pending),
            "completed"       => nameof(OrderStatus.Approved),
            "cancelled"       => nameof(OrderStatus.Rejected),
            "paymentfailed"   => nameof(OrderStatus.PaymentFailed),
            "paymentrecieved" => nameof(OrderStatus.PaymentRecieved),
            "paymentmismatch" => nameof(OrderStatus.PaymentMismatch),
            "customorder"     => nameof(OrderStatus.CustomOrder),
            _                 => null
        };

        var query = orderRepository.GetQueryable(statusFilter)
            .OrderByDescending(x => x.CreateAt);

        var orders = await PagedList<OrderDto>.ToPagedList(
            query, request.OrderParams.PageNumber, request.OrderParams.PageSize);

        return Result<PagedList<OrderDto>>.Success(orders);
    }
}
