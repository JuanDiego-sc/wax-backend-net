using Domain.Entities;
using Domain.OrderAggregate;
using BillingAddress = Domain.Entities.BillingAddress;

namespace Application.Orders.DTOs;

public record CreateOrderDto
{
    public required PaymentSummary PaymentSummary { get; set; }
}
