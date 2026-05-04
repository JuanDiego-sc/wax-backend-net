namespace Application.Orders.DTOs;

public record OrderDto
{
    public required string Id { get; set; }
    public required string BuyerEmail { get; set; }
    public required BillingAddressDto BillingAddress { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = [];
    public long Subtotal { get; set; }
    public long DeliveryFee { get; set; }
    public long Discount { get; set; }
    public long Total { get; set; }
    public required string OrderStatus { get; set; } 
    public required PaymentSummaryDto PaymentSummary { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
