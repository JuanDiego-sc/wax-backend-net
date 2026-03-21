using Domain.OrderAggregate;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void GetTotal_WithNoDiscount_ReturnsSumOfSubtotalAndDeliveryFee()
    {
        var order = OrderFixtures.CreateOrder(subtotal: 5000, deliveryFee: 500, discount: 0);

        var total = order.GetTotal();

        total.Should().Be(5500);
    }

    [Fact]
    public void GetTotal_WithDiscount_SubtractsDiscountFromSum()
    {
        var order = OrderFixtures.CreateOrder(subtotal: 5000, deliveryFee: 500, discount: 200);

        var total = order.GetTotal();

        total.Should().Be(5300);
    }

    [Fact]
    public void GetTotal_WithZeroDeliveryFee_ReturnsSubtotalMinusDiscount()
    {
        var order = OrderFixtures.CreateOrder(subtotal: 10000, deliveryFee: 0, discount: 500);

        var total = order.GetTotal();

        total.Should().Be(9500);
    }
}
