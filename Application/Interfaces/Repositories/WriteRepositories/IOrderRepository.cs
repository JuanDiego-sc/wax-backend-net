using Domain.OrderAggregate;

namespace Application.Interfaces.Repositories.WriteRepositories;

public interface IOrderRepository
{
    IQueryable<Order> GetQueryable();
    Task<Order?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    Task<Order?> GetByOrderIdAsync(string orderId, CancellationToken cancellationToken = default);
    void Add(Order order);
}
