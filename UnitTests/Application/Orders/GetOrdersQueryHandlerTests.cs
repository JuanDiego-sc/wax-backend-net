
using Application.Interfaces.Repositories.ReadRepositories;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using Application.Orders.Queries;
using Domain.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using MockQueryable;
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
        var orders = new List<Order>
        {
            OrderFixtures.CreateOrder(),
            OrderFixtures.CreateOrder(),
            OrderFixtures.CreateOrder()
        };
        context.Orders.AddRange(orders);
        await context.SaveChangesAsync();

        var orderDtos = context.Orders.Include(o => o.OrderItems).AsEnumerable().Select(o => o.ToDto()).ToList();
        var mockQueryable = orderDtos.BuildMock();

        var repoMock = new Mock<IOrderReadRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(mockQueryable);

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

        var orderDtos = context.Orders.Include(o => o.OrderItems).AsEnumerable().Select(o => o.ToDto()).ToList();
        var mockQueryable = orderDtos.BuildMock();

        var repoMock = new Mock<IOrderReadRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(mockQueryable);

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

        var orderDtos = context.Orders.Include(o => o.OrderItems).AsEnumerable().Select(o => o.ToDto()).ToList();
        var mockQueryable = orderDtos.BuildMock();

        var repoMock = new Mock<IOrderReadRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(mockQueryable);

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
