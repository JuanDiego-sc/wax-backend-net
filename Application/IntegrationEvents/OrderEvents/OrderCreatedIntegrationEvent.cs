namespace Application.IntegrationEvents.OrderEvents;

public class OrderCreatedIntegrationEvent
{
    public required string OrderId { get; init; }
    public required string BuyerEmail { get; init; }
    public required string OrderStatus { get; init; }
    public long Subtotal { get; init; }
    public long DeliveryFee { get; init; }
    public long Total { get; init; }

    public required string BillingName { get; init; }
    public required string BillingLine1 { get; init; }
    public string? BillingLine2 { get; init; }
    public required string BillingCity { get; init; }
    public required string BillingState { get; init; }
    public required string BillingPostalCode { get; init; }
    public required string BillingCountry { get; init; }

    public int PaymentLast4 { get; init; }
    public required string PaymentBrand { get; init; }
    public int PaymentExpMonth { get; init; }
    public int PaymentExpYear { get; init; }

    public required string OrderItems { get; init; }
    public required string PaymentIntentId { get; init; }

    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
    
