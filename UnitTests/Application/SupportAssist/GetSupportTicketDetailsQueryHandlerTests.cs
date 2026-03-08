using Application.Interfaces.Repositories;
using Application.SupportAssist.Queries;
using Domain.SupportAssistAggregate;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.SupportAssist;

public class GetSupportTicketDetailsQueryHandlerTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_WhenTicketNotFound_ReturnsFailure()
    {
        using var context = CreateInMemoryContext();

        var repoMock = new Mock<ISupportTicketRepository>();
        repoMock.Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportTicket?)null);

        var handler = new GetSupportTicketDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(
            new GetSupportTicketDetailsQuery { TicketId = "missing" },
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Ticket not found");
    }

    [Fact]
    public async Task Handle_WhenTicketFound_ReturnsMappedDto()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket(
            subject: "Test Ticket",
            description: "Test Description");

        var repoMock = new Mock<ISupportTicketRepository>();
        repoMock.Setup(r => r.GetTicketByIdAsync(ticket.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var handler = new GetSupportTicketDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(
            new GetSupportTicketDetailsQuery { TicketId = ticket.Id },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Id.Should().Be(ticket.Id);
        result.Value.Subject.Should().Be("Test Ticket");
        result.Value.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task Handle_WhenTicketFound_MapsUserInformation()
    {
        var user = SupportTicketFixtures.CreateUser(userName: "testuser", email: "test@example.com");
        var ticket = SupportTicketFixtures.CreateSupportTicket(user: user);

        var repoMock = new Mock<ISupportTicketRepository>();
        repoMock.Setup(r => r.GetTicketByIdAsync(ticket.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var handler = new GetSupportTicketDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(
            new GetSupportTicketDetailsQuery { TicketId = ticket.Id },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.UserId.Should().Be(user.Id);
        result.Value.UserEmail.Should().Be("testuser");
        result.Value.UserFullName.Should().Be("testuser");
    }

    [Fact]
    public async Task Handle_WhenTicketFound_MapsStatusAndCategory()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket(
            status: TicketStatus.Open,
            category: TicketCategory.PaymentIssue);

        var repoMock = new Mock<ISupportTicketRepository>();
        repoMock.Setup(r => r.GetTicketByIdAsync(ticket.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        var handler = new GetSupportTicketDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(
            new GetSupportTicketDetailsQuery { TicketId = ticket.Id },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("Open");
        result.Value.Category.Should().Be("PaymentIssue");
    }
}
