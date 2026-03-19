using Application.Interfaces.Repositories.ReadRepositories;
using Application.SupportAssist.DTOs;
using Application.SupportAssist.Queries;

namespace UnitTests.Application.SupportAssist;

public class GetSupportTicketDetailsQueryHandlerTests
{
    private static SupportTicketDto CreateTicketDto(
        string? id = null,
        string? userId = null,
        string? userEmail = null,
        string? userFullName = null,
        string? orderId = null,
        string? subject = null,
        string? description = null,
        string? status = null,
        string? category = null)
    {
        return new SupportTicketDto
        {
            Id = id ?? Guid.NewGuid().ToString(),
            UserId = userId ?? Guid.NewGuid().ToString(),
            UserEmail = userEmail ?? "test@example.com",
            UserFullName = userFullName ?? "Test User",
            OrderId = orderId ?? Guid.NewGuid().ToString(),
            Subject = subject ?? "Test Subject",
            Description = description ?? "Test Description",
            Status = status ?? "Open",
            Category = category ?? "Other",
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Handle_WhenTicketNotFound_ReturnsFailure()
    {
        var repoMock = new Mock<ISupportTicketReadRepository>();
        repoMock.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportTicketDto?)null);

        var handler = new GetSupportTicketDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(
            new GetSupportTicketDetailsQuery { TicketId = "missing" },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Ticket not found");
    }

    [Fact]
    public async Task Handle_WhenTicketFound_ReturnsDto()
    {
        var ticketDto = CreateTicketDto(
            id: "ticket-123",
            subject: "Test Ticket",
            description: "Test Description");

        var repoMock = new Mock<ISupportTicketReadRepository>();
        repoMock.Setup(r => r.GetByIdAsync("ticket-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticketDto);

        var handler = new GetSupportTicketDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(
            new GetSupportTicketDetailsQuery { TicketId = "ticket-123" },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be("ticket-123");
        result.Value.Subject.Should().Be("Test Ticket");
        result.Value.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task Handle_WhenTicketFound_ReturnsUserInformation()
    {
        var ticketDto = CreateTicketDto(
            id: "ticket-456",
            userId: "user-123",
            userEmail: "test@example.com",
            userFullName: "John Doe");

        var repoMock = new Mock<ISupportTicketReadRepository>();
        repoMock.Setup(r => r.GetByIdAsync("ticket-456", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticketDto);

        var handler = new GetSupportTicketDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(
            new GetSupportTicketDetailsQuery { TicketId = "ticket-456" },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.UserId.Should().Be("user-123");
        result.Value.UserEmail.Should().Be("test@example.com");
        result.Value.UserFullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task Handle_WhenTicketFound_ReturnsStatusAndCategory()
    {
        var ticketDto = CreateTicketDto(
            id: "ticket-789",
            status: "Open",
            category: "PaymentIssue");

        var repoMock = new Mock<ISupportTicketReadRepository>();
        repoMock.Setup(r => r.GetByIdAsync("ticket-789", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticketDto);

        var handler = new GetSupportTicketDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(
            new GetSupportTicketDetailsQuery { TicketId = "ticket-789" },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("Open");
        result.Value.Category.Should().Be("PaymentIssue");
    }
}
