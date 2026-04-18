using Application.Core;
using Application.Core.Validations;
using Application.Interfaces.Repositories.ReadRepositories;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries;

public class GetOrderDetailsQueryHandler(IOrderReadRepository orderRepository) 
    : IRequestHandler<GetOrderDetailsQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetOrderByIdAsync(request.OrderId, cancellationToken);

        if (order == null)
            return Result<OrderDto>.Failure("Order not found", 404);

        return Result<OrderDto>.Success(order);
    }
}
