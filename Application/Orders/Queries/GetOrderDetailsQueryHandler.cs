using System;
using Application.Core;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Orders.Queries;

public class GetOrderDetailsQueryHandler(AppDbContext context) : IRequestHandler<GetOrderDetailsQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        var order = await context.Orders
            .AsNoTracking()
            .ProjectToDto()
            .Where(x => x.Id == request.OrderId)
            .FirstOrDefaultAsync(cancellationToken);

        if(order == null)
            return Result<OrderDto>.Failure("Order not found");
        
        return Result<OrderDto>.Success(order);
    }
}
