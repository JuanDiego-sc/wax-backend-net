using System;

namespace Domain.OrderAggregate;

public class Order
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string BuyerEmail { get; set; }
    public required BillingAddress ShippingAddress { get; set; }
    public List<OrderItem> OrderItems { get; set; } = [];
    public long Subtotal { get; set; }
    public long DeliveryFee { get; set; }
    public long Discount { get; set; }
    public required string PaymentIntentId { get; set; } 
    public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;
    public required PaymentSummary PaymentSummary { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public long GetTotal()
    {
        return Subtotal + DeliveryFee - Discount;
    }
}
