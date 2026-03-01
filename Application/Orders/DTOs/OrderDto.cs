using System;
using Domain.OrderAggregate;

namespace Application.Orders.DTOs;

public record OrderDto
{
    public required string Id { get; set; }
    public required string BuyerEmail { get; set; }
    public required BillingAddress ShippingAddress { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = [];
    public long Subtotal { get; set; }
    public long DeliveryFee { get; set; }
    public long Discount { get; set; }
    public long Total { get; set; }
    public required string OrderStatus { get; set; } 
    public required PaymentSummary PaymentSummary { get; set; }

    public DateTime CreateAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
