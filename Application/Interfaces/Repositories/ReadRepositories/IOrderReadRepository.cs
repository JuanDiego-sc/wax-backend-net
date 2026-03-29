using Application.Orders.DTOs;

namespace Application.Interfaces.Repositories.ReadRepositories;

public interface IOrderReadRepository
{
    Task<OrderDto?> GetOrderByIdAsync(string id,  CancellationToken cancellationToken = default);
    Task<OrderDto?> GetByPaymentIntentIdAsync(string paymentIntentId, CancellationToken cancellationToken = default);
    IQueryable<OrderDto> GetQueryable();
}