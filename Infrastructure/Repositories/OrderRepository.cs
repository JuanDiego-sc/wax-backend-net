using Application.Interfaces.Repositories;
using Domain.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public IQueryable<Order> GetQueryable()
    {
        return context.Orders.AsQueryable();
    }

    public async Task<Order?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken)
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

    public void Update(Order order)
    {
        context.Orders.Update(order);
    }
}
