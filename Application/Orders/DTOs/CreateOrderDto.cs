using Domain.Entities;
using Domain.OrderAggregate;

namespace Application.Orders.DTOs;

public record CreateOrderDto
{
    public required Address BillingAddress { get; set; }
    public required PaymentSummary PaymentSummary { get; set; }
}
