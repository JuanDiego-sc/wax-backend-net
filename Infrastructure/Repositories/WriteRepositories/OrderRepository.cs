using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WriteRepositores;
using Domain.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories;

public class OrderRepository(WriteDbContext context) : IOrderRepository
{
    public IQueryable<Order> GetQueryable()
    {
        return context.Orders.AsQueryable();
    }

    public async Task<Order?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.PaymentIntentId == paymentIntentId, cancellationToken);
    }

    public async Task<Order?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .FindAsync([orderId], cancellationToken);
    }

    public void Add(Order order)
    {
        context.Orders.Add(order);
    }
    
}
