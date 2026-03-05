using Application.Interfaces.Repositories;
using Application.Orders.Queries;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Orders;

public class GetOrderDetailsQueryHandlerTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new AppDbContext(options);
        return context;
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ReturnsFailureWith404()
    {
        using var context = CreateInMemoryContext();

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.Orders.Include(o => o.OrderItems).AsQueryable());

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

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.Orders.Include(o => o.OrderItems).AsQueryable());

        var handler = new GetOrderDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(new GetOrderDetailsQuery { OrderId = order.Id }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.BuyerEmail.Should().Be("buyer@example.com");
    }
}
