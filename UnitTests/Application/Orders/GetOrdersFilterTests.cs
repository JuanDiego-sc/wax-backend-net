using Application.Interfaces.Repositories.ReadRepositories;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using Application.Orders.Queries;
using MockQueryable;

namespace UnitTests.Application.Orders;

public class GetOrdersFilterTests
{
    private static Mock<IOrderReadRepository> BuildRepoMock(out string? capturedFilter)
    {
        string? captured = null;
        var repoMock = new Mock<IOrderReadRepository>();
        repoMock
            .Setup(r => r.GetQueryable(It.IsAny<string?>()))
            .Callback<string?>(f => captured = f)
            .Returns(new List<OrderDto>().BuildMock());

        capturedFilter = captured;
        return repoMock;
    }

    private static async Task<string?> ExecuteAndCaptureFilter(string? filterInput)
    {
        string? captured = null;
        var repoMock = new Mock<IOrderReadRepository>();
        repoMock
            .Setup(r => r.GetQueryable(It.IsAny<string?>()))
            .Callback<string?>(f => captured = f)
            .Returns(new List<OrderDto>().BuildMock());

        var handler = new GetOrdersQueryHandler(repoMock.Object);
        var query = new GetOrdersQuery
        {
            OrderParams = new OrderParams { Filter = filterInput, PageSize = 10 }
        };

        await handler.Handle(query, CancellationToken.None);
        return captured;
    }

    [Fact]
    public async Task Handle_WhenFilterIsPending_CallsGetQueryableWithPending()
    {
        var captured = await ExecuteAndCaptureFilter("pending");

        captured.Should().Be("Pending");
    }

    [Fact]
    public async Task Handle_WhenFilterIsCompleted_CallsGetQueryableWithApproved()
    {
        var captured = await ExecuteAndCaptureFilter("completed");

        captured.Should().Be("Approved");
    }

    [Fact]
    public async Task Handle_WhenFilterIsCancelled_CallsGetQueryableWithRejected()
    {
        var captured = await ExecuteAndCaptureFilter("cancelled");

        captured.Should().Be("Rejected");
    }

    [Fact]
    public async Task Handle_WhenFilterIsPaymentFailed_CallsGetQueryableWithPaymentFailed()
    {
        var captured = await ExecuteAndCaptureFilter("paymentfailed");

        captured.Should().Be("PaymentFailed");
    }

    [Fact]
    public async Task Handle_WhenFilterIsPaymentRecieved_CallsGetQueryableWithPaymentRecieved()
    {
        var captured = await ExecuteAndCaptureFilter("paymentrecieved");

        captured.Should().Be("PaymentRecieved");
    }

    [Fact]
    public async Task Handle_WhenFilterIsPaymentMismatch_CallsGetQueryableWithPaymentMismatch()
    {
        var captured = await ExecuteAndCaptureFilter("paymentmismatch");

        captured.Should().Be("PaymentMismatch");
    }

    [Fact]
    public async Task Handle_WhenFilterIsCustomOrder_CallsGetQueryableWithCustomOrder()
    {
        var captured = await ExecuteAndCaptureFilter("customorder");

        captured.Should().Be("CustomOrder");
    }

    [Fact]
    public async Task Handle_WhenFilterIsUnknown_CallsGetQueryableWithNull()
    {
        var captured = await ExecuteAndCaptureFilter("someunknownvalue");

        captured.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenFilterIsNull_CallsGetQueryableWithNull()
    {
        var captured = await ExecuteAndCaptureFilter(null);

        captured.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenFilterIsPendingUpperCase_CallsGetQueryableWithNull()
    {
        // The handler does ToLower() before switching, so "PENDING" matches "pending"
        var captured = await ExecuteAndCaptureFilter("PENDING");

        captured.Should().Be("Pending");
    }
}
