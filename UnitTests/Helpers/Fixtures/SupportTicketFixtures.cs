using Domain.Entities;
using Domain.OrderAggregate;
using Domain.SupportAssistAggregate;

namespace UnitTests.Helpers.Fixtures;

public static class SupportTicketFixtures
{
    public static SupportTicket CreateSupportTicket(
        string? id = null,
        string? userId = null,
        string? orderId = null,
        TicketCategory category = TicketCategory.PaymentIssue,
        TicketStatus status = TicketStatus.Open,
        string subject = "Test Subject",
        string description = "Test Description",
        User? user = null,
        Order? order = null)
    {
        var actualUser = user ?? CreateUser(userId);
        var actualOrder = order ?? OrderFixtures.CreateOrder();

        return new SupportTicket
        {
            Id = id ?? Guid.NewGuid().ToString(),
            UserId = actualUser.Id,
            OrderId = orderId ?? actualOrder.Id,
            Category = category,
            Status = status,
            Subject = subject,
            Description = description,
            User = actualUser,
            Order = actualOrder
        };
    }

    public static User CreateUser(
        string? id = null,
        string userName = "testuser",
        string email = "test@example.com")
    {
        return new User
        {
            Id = id ?? Guid.NewGuid().ToString(),
            UserName = userName,
            Email = email
        };
    }
}
