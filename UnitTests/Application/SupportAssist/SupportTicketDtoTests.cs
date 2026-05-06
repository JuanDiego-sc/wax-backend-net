using Application.SupportAssist.DTOs;
using Domain.SupportAssistAggregate;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.SupportAssist;

public class SupportTicketDtoTests
{
    [Fact]
    public void FromEntity_MapsAllPropertiesCorrectly()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket(
            category: TicketCategory.OrderIssue,
            status: TicketStatus.Open,
            subject: "Test subject",
            description: "Test description");

        var dto = SupportTicketDto.FromEntity(ticket);

        dto.Id.Should().Be(ticket.Id);
        dto.Subject.Should().Be("Test subject");
        dto.Description.Should().Be("Test description");
        dto.UserId.Should().Be(ticket.UserId);
        dto.OrderId.Should().Be(ticket.OrderId);
        dto.CreatedAt.Should().Be(ticket.CreatedAt);
    }

    [Fact]
    public void FromEntity_ConvertsStatusToString()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket(status: TicketStatus.InProgress);

        var dto = SupportTicketDto.FromEntity(ticket);

        dto.Status.Should().Be(nameof(TicketStatus.InProgress));
    }

    [Fact]
    public void FromEntity_ConvertsCategoryToString()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket(category: TicketCategory.PaymentIssue);

        var dto = SupportTicketDto.FromEntity(ticket);

        dto.Category.Should().Be(nameof(TicketCategory.PaymentIssue));
    }

    [Fact]
    public void FromEntity_WhenStatusIsClosed_ConvertsToClosed()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket(status: TicketStatus.Closed);

        var dto = SupportTicketDto.FromEntity(ticket);

        dto.Status.Should().Be(nameof(TicketStatus.Closed));
    }

    [Fact]
    public void FromEntity_SetsUserEmailFromUserName()
    {
        var user = SupportTicketFixtures.CreateUser(userName: "user@example.com");
        var ticket = SupportTicketFixtures.CreateSupportTicket(user: user);

        var dto = SupportTicketDto.FromEntity(ticket);

        dto.UserEmail.Should().Be("user@example.com");
        dto.UserFullName.Should().Be("user@example.com");
    }
}
