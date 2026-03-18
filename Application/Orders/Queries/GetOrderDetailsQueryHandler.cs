using Application.Core;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Orders.Queries;

public class GetOrderDetailsQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrderDetailsQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetQueryable()
            .AsNoTracking()
            .ProjectToDto()
            .Where(x => x.Id == request.OrderId)
            .FirstOrDefaultAsync(cancellationToken);

        if (order == null)
            return Result<OrderDto>.Failure("Order not found", 404);

        return Result<OrderDto>.Success(order);
    }
}
