using Domain.Entities;
using Domain.OrderAggregate;

namespace UnitTests.Helpers.Fixtures;

public static class OrderFixtures
{
    public static Order CreateOrder(
        string? paymentIntentId = null,
        string buyerEmail = "buyer@test.com",
        long subtotal = 5000,
        long deliveryFee = 500,
        long discount = 0,
        OrderStatus status = OrderStatus.Pending,
        List<OrderItem>? items = null)
    {
        var intentId = paymentIntentId ?? Guid.NewGuid().ToString();
        var addressId = Guid.NewGuid().ToString();

        return new Order
        {
            PaymentIntentId = intentId,
            BuyerEmail = buyerEmail,
            Subtotal = subtotal,
            DeliveryFee = deliveryFee,
            Discount = discount,
            OrderStatus = status,
            OrderItems = items ?? [CreateOrderItem()],
            BillingAddress = CreateBillingAddress(),
            AddressId = addressId,
            PaymentSummary = CreatePaymentSummary()
        };
    }

    public static OrderItem CreateOrderItem(
        string? productId = null,
        long price = 1500,
        int quantity = 2)
    {
        return new OrderItem
        {
            ItemOrdered = ProductFixtures.CreateProductOrderItem(productId),
            Price = price,
            Quantity = quantity
        };
    }

    public static Address CreateBillingAddress()
    {
        return new Address
        {
            Name = "Test User",
            Line1 = "123 Test Street",
            Line2 = null,
            City = "Test City",
            State = "TS",
            PostalCode = "12345",
            Country = "US"
        };
    }

    public static PaymentSummary CreatePaymentSummary()
    {
        return new PaymentSummary
        {
            Last4 = 4242,
            Brand = "Visa",
            ExpMonth = 12,
            ExpYear = 2026
        };
    }
}
