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

    private static Mock<IOrderReadRepository> SetupRepoMock(List<OrderDto> dtos, string? statusFilter = null)
    {
        var mock = new Mock<IOrderReadRepository>();
        var filtered = statusFilter != null
            ? dtos.Where(d => d.OrderStatus == statusFilter).ToList()
            : dtos;
        mock.Setup(r => r.GetQueryable(It.IsAny<string?>())).Returns(filtered.BuildMock());
        return mock;
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

        var dtos = context.Orders.Include(o => o.OrderItems).AsEnumerable().Select(o => o.ToDto()).ToList();
        var repoMock = SetupRepoMock(dtos);

        var handler = new GetOrdersQueryHandler(repoMock.Object);
        var query = new GetOrdersQuery { OrderParams = new() { PageSize = 10 } };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
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

        var dtos = context.Orders.Include(o => o.OrderItems).AsEnumerable().Select(o => o.ToDto()).ToList();
        var repoMock = SetupRepoMock(dtos, nameof(OrderStatus.Pending));

        var handler = new GetOrdersQueryHandler(repoMock.Object);
        var query = new GetOrdersQuery { OrderParams = new() { Filter = "pending", PageSize = 10 } };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ReturnsCorrectPaginationMetadata()
    {
        using var context = CreateInMemoryContext();
        for (var i = 0; i < 5; i++)
            context.Orders.Add(OrderFixtures.CreateOrder());
        await context.SaveChangesAsync();

        var dtos = context.Orders.Include(o => o.OrderItems).AsEnumerable().Select(o => o.ToDto()).ToList();
        var repoMock = SetupRepoMock(dtos);

        var handler = new GetOrdersQueryHandler(repoMock.Object);
        var query = new GetOrdersQuery { OrderParams = new() { PageSize = 3, PageNumber = 1 } };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(3);
        result.Value!.Metadata.TotalCount.Should().Be(5);
        result.Value.Metadata.TotalPages.Should().Be(2);
    }
}
