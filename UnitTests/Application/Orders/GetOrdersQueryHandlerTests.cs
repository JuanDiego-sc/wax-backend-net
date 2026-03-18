
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Orders.Queries;
using Domain.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Orders;

public class GetOrdersQueryHandlerTests
{
    private static WriteDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new WriteDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsAllOrdersWithoutFilter()
    {
        using var context = CreateInMemoryContext();
        context.Orders.AddRange(
            OrderFixtures.CreateOrder(),
            OrderFixtures.CreateOrder(),
            OrderFixtures.CreateOrder());
        await context.SaveChangesAsync();

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.Orders.Include(o => o.OrderItems).AsQueryable());

        var handler = new GetOrdersQueryHandler(repoMock.Object);
        var query = new GetOrdersQuery
        {
            OrderParams = new() { PageSize = 10, StartDate = DateTime.UtcNow.AddDays(-1) }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ReturnsOnlyMatchingOrders()
    {
        using var context = CreateInMemoryContext();
        context.Orders.AddRange(
            OrderFixtures.CreateOrder(status: OrderStatus.Pending),
            OrderFixtures.CreateOrder(status: OrderStatus.Approved),
            OrderFixtures.CreateOrder(status: OrderStatus.Pending));
        await context.SaveChangesAsync();

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.Orders.Include(o => o.OrderItems).AsQueryable());

        var handler = new GetOrdersQueryHandler(repoMock.Object);
        var query = new GetOrdersQuery
        {
            OrderParams = new() { Filter = "pending", PageSize = 10, StartDate = DateTime.UtcNow.AddDays(-1) }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WhenExceedsPageSize_SetsNextCursor()
    {
        using var context = CreateInMemoryContext();
        for (var i = 0; i < 4; i++)
            context.Orders.Add(OrderFixtures.CreateOrder());
        await context.SaveChangesAsync();

        var repoMock = new Mock<IOrderRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.Orders.Include(o => o.OrderItems).AsQueryable());

        var handler = new GetOrdersQueryHandler(repoMock.Object);
        var query = new GetOrdersQuery
        {
            OrderParams = new() { PageSize = 3, StartDate = DateTime.UtcNow.AddDays(-1) }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(3);
        result.Value.NextCursor.Should().NotBeNull();
    }
}
