using System;
using Domain.OrderAggregate;

namespace Application.Orders.DTOs;

public record CreateOrderDto
{
    public required BillingAddress BillingAddress { get; set; }
    public required PaymentSummary PaymentSummary { get; set; }
}
