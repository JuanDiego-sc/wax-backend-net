using Application.Core;
using Application.Core.Pagination;
using Application.Core.Validations;
using Application.Interfaces.Repositories.ReadRepositories;
using Application.Interfaces.Services;
using Application.Orders.DTOs;
using Domain.OrderAggregate;
using MediatR;

namespace Application.Orders.Queries;

public class GetMyOrdersQueryHandler(
    IOrderReadRepository repository,
    IUserAccessor userAccessor)
    : IRequestHandler<GetMyOrdersQuery, Result<PagedList<OrderDto>>>
{
    public async Task<Result<PagedList<OrderDto>>> Handle(
        GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        var userId = userAccessor.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Result<PagedList<OrderDto>>.Failure("User identity could not be resolved.", 401);

        var statusFilter = request.Params.Filter?.ToLower() switch
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

        var query = repository.GetQueryable(statusFilter, userId)
            .OrderByDescending(x => x.CreatedAt);

        var paged = await PagedList<OrderDto>.ToPagedList(
            query, request.Params.PageNumber, request.Params.PageSize);

        return Result<PagedList<OrderDto>>.Success(paged);
    }
}
