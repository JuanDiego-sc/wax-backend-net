namespace Persistence.ReadModels;

public class OrderReadModel
{
    public required string Id { get; set; }
    public required string BuyerEmail { get; set; }
    public required string OrderStatus { get; set; }
    public long Subtotal { get; set; }
    public long DeliveryFee { get; set; }
    public long Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    //denormalized
    public required string BillingName { get; set; }
    public required string BillingLine1 { get; set; }
    public string? BillingLine2 { get; set; }
    public required string BillingCity { get; set; }
    public required string BillingState { get; set; }
    public required string BillingPostalCode { get; set; }
    public required string BillingCountry { get; set; }

    //denormalized
    public int PaymentLast4 { get; set; }
    public required string PaymentBrand { get; set; }
    public int PaymentExpMonth { get; set; }
    public int PaymentExpYear { get; set; }

    // Optimized to JSON
    public required string OrderItems { get; set; }
    public required string PaymentIntentId { get; set; }

    public DateTime LastSyncedAt { get; set; }
}