using Application.Interfaces.Repositories.ReadRepositories;
using Application.Interfaces.Services;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using Application.Orders.Queries;
using MockQueryable;

namespace UnitTests.Application.Orders;

public class GetMyOrdersQueryHandlerTests
{
    private const string UserId = "user-order-abc";

    private static OrderDto BuildOrderDto(string? userId = null, string orderStatus = "Pending") => new()
    {
        Id = Guid.NewGuid().ToString(),
        BuyerEmail = "buyer@test.com",
        UserId = userId ?? UserId,
        OrderStatus = orderStatus,
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
            PostalCode = "12345"
        },
        PaymentSummary = new PaymentSummaryDto
        {
            Last4 = 4242,
            Brand = "visa",
            ExpMonth = 12,
            ExpYear = 2026
        }
    };

    private static (Mock<IOrderReadRepository> Repo, Mock<IUserAccessor> UserAccessor, GetMyOrdersQueryHandler Handler) Build(
        List<OrderDto> dtos)
    {
        var repo = new Mock<IOrderReadRepository>();
        repo.Setup(r => r.GetQueryable(It.IsAny<string?>(), It.IsAny<string?>()))
            .Returns<string?, string?>((_, _) => dtos.BuildMock());

        var userAccessor = new Mock<IUserAccessor>();
        userAccessor.Setup(u => u.GetUserId()).Returns(UserId);

        var handler = new GetMyOrdersQueryHandler(repo.Object, userAccessor.Object);
        return (repo, userAccessor, handler);
    }

    [Fact]
    public async Task Handle_PassesUserIdToRepository()
    {
        var dtos = new List<OrderDto> { BuildOrderDto() };
        var (repo, _, handler) = Build(dtos);
        var query = new GetMyOrdersQuery(new OrderParams { PageSize = 10, PageNumber = 1 });

        await handler.Handle(query, CancellationToken.None);

        repo.Verify(r => r.GetQueryable(It.IsAny<string?>(), UserId), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsPagedResultWithMatchingOrders()
    {
        var dtos = new List<OrderDto>
        {
            BuildOrderDto(),
            BuildOrderDto(),
            BuildOrderDto()
        };
        var (_, _, handler) = Build(dtos);
        var query = new GetMyOrdersQuery(new OrderParams { PageSize = 10, PageNumber = 1 });

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        var dtos = Enumerable.Range(0, 10).Select(_ => BuildOrderDto()).ToList();
        var (_, _, handler) = Build(dtos);
        var query = new GetMyOrdersQuery(new OrderParams { PageSize = 4, PageNumber = 2 });

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(4);
        result.Value.Metadata.TotalCount.Should().Be(10);
        result.Value.Metadata.CurrentPage.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_PassesStatusToRepository()
    {
        var dtos = new List<OrderDto> { BuildOrderDto(orderStatus: "Pending") };
        var (repo, _, handler) = Build(dtos);
        var query = new GetMyOrdersQuery(new OrderParams { Filter = "pending", PageSize = 10, PageNumber = 1 });

        await handler.Handle(query, CancellationToken.None);

        repo.Verify(r => r.GetQueryable(It.IsNotNull<string>(), UserId), Times.Once);
    }

    [Fact]
    public async Task Handle_CallsGetUserIdOnce()
    {
        var (_, userAccessor, handler) = Build(new List<OrderDto>());
        var query = new GetMyOrdersQuery(new OrderParams { PageSize = 10, PageNumber = 1 });

        await handler.Handle(query, CancellationToken.None);

        userAccessor.Verify(u => u.GetUserId(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoOrders_ReturnsEmptyPagedList()
    {
        var (_, _, handler) = Build(new List<OrderDto>());
        var query = new GetMyOrdersQuery(new OrderParams { PageSize = 10, PageNumber = 1 });

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ReturnsFailure()
    {
        var repo = new Mock<IOrderReadRepository>();
        var userAccessor = new Mock<IUserAccessor>();
        userAccessor.Setup(u => u.GetUserId()).Returns(default(string));
        var handler = new GetMyOrdersQueryHandler(repo.Object, userAccessor.Object);
        var query = new GetMyOrdersQuery(new OrderParams { PageSize = 10, PageNumber = 1 });

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        repo.Verify(r => r.GetQueryable(It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
    }
}
