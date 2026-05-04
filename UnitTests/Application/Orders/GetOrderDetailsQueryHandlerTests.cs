using Application.Interfaces.Repositories.ReadRepositories;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using Application.Orders.Queries;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Orders;

public class GetOrderDetailsQueryHandlerTests
{
    private static WriteDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new WriteDbContext(options);
        return context;
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ReturnsFailureWith404()
    {
        var repoMock = new Mock<IOrderReadRepository>();
        repoMock
            .Setup(r => r.GetOrderByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);

        var handler = new GetOrderDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(new GetOrderDetailsQuery { OrderId = "missing" }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Order not found");
        result.Code.Should().Be(404);
    }

    [Fact]
    public async Task Handle_WhenOrderFound_ReturnsMappedDto()
    {
        using var context = CreateInMemoryContext();
        var order = OrderFixtures.CreateOrder(buyerEmail: "buyer@example.com");
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var expectedDto = new OrderDto
        {
            Id = "test-order-id",
            BuyerEmail = "buyer@example.com",
            OrderStatus = "Pending",
            Subtotal = 1000,
            DeliveryFee = 100,
            Total = 1100,
            CreatedAt = DateTime.UtcNow,
            BillingAddress = new BillingAddressDto
            {
                Name = "Test User",
                Line1 = "Calle 1",
                City = "Quito",
                Country = "EC",
                State = "Calle 2",
                PostalCode = "12345",
            },
            PaymentSummary = new PaymentSummaryDto
            {
                Last4 = 4242,
                Brand = "visa",
                ExpMonth = 12,
                ExpYear = 2026
            },
            OrderItems = []
        };

        var repoMock = new Mock<IOrderReadRepository>();
        repoMock
            .Setup(r => r.GetOrderByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var handler = new GetOrderDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(new GetOrderDetailsQuery { OrderId = order.Id }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.BuyerEmail.Should().Be("buyer@example.com");
    }
}
