using Application.Notifications;
using Application.Notifications.Requests;

namespace UnitTests.Application.Notifications;

public class EmailRequestTests
{
    [Fact]
    public void AccountConfirmationEmailRequest_SetsAllProperties()
    {
        var request = new AccountConfirmationEmailRequest
        {
            ToEmail = "user@example.com",
            ToName = "John Doe",
            ConfirmationLink = "https://example.com/confirm?token=abc"
        };

        request.ToEmail.Should().Be("user@example.com");
        request.ToName.Should().Be("John Doe");
        request.ConfirmationLink.Should().Be("https://example.com/confirm?token=abc");
    }

    [Fact]
    public void AccountConfirmationEmailRequest_EmailType_ReturnsAccountConfirmation()
    {
        var request = new AccountConfirmationEmailRequest
        {
            ToEmail = "user@example.com",
            ToName = "John",
            ConfirmationLink = "https://example.com/confirm"
        };

        request.EmailType.Should().Be(EmailType.AccountConfirmation);
    }

    [Fact]
    public void OrderStatusChangedEmailRequest_SetsAllProperties()
    {
        var request = new OrderStatusChangedEmailRequest
        {
            ToEmail = "user@example.com",
            ToName = "Jane",
            OrderNumber = "ORDER-001",
            OldStatus = "Pending",
            NewStatus = "Approved",
            Total = 5000
        };

        request.ToEmail.Should().Be("user@example.com");
        request.ToName.Should().Be("Jane");
        request.OrderNumber.Should().Be("ORDER-001");
        request.OldStatus.Should().Be("Pending");
        request.NewStatus.Should().Be("Approved");
        request.Total.Should().Be(5000);
    }

    [Fact]
    public void OrderStatusChangedEmailRequest_EmailType_ReturnsOrderStatusChanged()
    {
        var request = new OrderStatusChangedEmailRequest
        {
            ToEmail = "u@e.com",
            ToName = "U",
            OrderNumber = "O",
            OldStatus = "A",
            NewStatus = "B"
        };

        request.EmailType.Should().Be(EmailType.OrderStatusChanged);
    }

    [Fact]
    public void PasswordResetEmailRequest_SetsAllProperties()
    {
        var request = new PasswordResetEmailRequest
        {
            ToEmail = "user@example.com",
            ToName = "Alice",
            ResetLink = "https://example.com/reset?code=xyz",
            ResetCode = "XYZ123"
        };

        request.ToEmail.Should().Be("user@example.com");
        request.ToName.Should().Be("Alice");
        request.ResetLink.Should().Be("https://example.com/reset?code=xyz");
        request.ResetCode.Should().Be("XYZ123");
    }

    [Fact]
    public void PasswordResetEmailRequest_EmailType_ReturnsPasswordReset()
    {
        var request = new PasswordResetEmailRequest
        {
            ToEmail = "u@e.com",
            ToName = "U",
            ResetLink = "link",
            ResetCode = "code"
        };

        request.EmailType.Should().Be(EmailType.PasswordReset);
    }

    [Fact]
    public void PaymentConfirmedEmailRequest_SetsAllProperties()
    {
        var request = new PaymentConfirmedEmailRequest
        {
            ToEmail = "user@example.com",
            ToName = "Bob",
            OrderNumber = "PAY-001",
            TotalAmount = 25000
        };

        request.ToEmail.Should().Be("user@example.com");
        request.ToName.Should().Be("Bob");
        request.OrderNumber.Should().Be("PAY-001");
        request.TotalAmount.Should().Be(25000);
    }

    [Fact]
    public void PaymentConfirmedEmailRequest_EmailType_ReturnsPaymentCompleted()
    {
        var request = new PaymentConfirmedEmailRequest
        {
            ToEmail = "u@e.com",
            ToName = "U",
            OrderNumber = "O",
            TotalAmount = 100
        };

        request.EmailType.Should().Be(EmailType.PaymentCompleted);
    }

    [Fact]
    public void SupportTicketCreatedEmailRequest_SetsAllProperties()
    {
        var request = new SupportTicketCreatedEmailRequest
        {
            ToEmail = "user@example.com",
            ToName = "Carol",
            OrderNumber = "TICKET-001",
            Subject = "My product is broken"
        };

        request.ToEmail.Should().Be("user@example.com");
        request.ToName.Should().Be("Carol");
        request.OrderNumber.Should().Be("TICKET-001");
        request.Subject.Should().Be("My product is broken");
    }

    [Fact]
    public void SupportTicketCreatedEmailRequest_EmailType_ReturnsSupportTicketCreated()
    {
        var request = new SupportTicketCreatedEmailRequest
        {
            ToEmail = "u@e.com",
            ToName = "U",
            OrderNumber = "O",
            Subject = "S"
        };

        request.EmailType.Should().Be(EmailType.SupportTicketCreated);
    }

    [Fact]
    public void SupportTicketUpdatedEmailRequest_SetsAllProperties()
    {
        var request = new SupportTicketUpdatedEmailRequest
        {
            ToEmail = "user@example.com",
            ToName = "Dave",
            OrderNumber = "TICKET-002",
            Subject = "Follow up",
            NewStatus = "InProgress"
        };

        request.ToEmail.Should().Be("user@example.com");
        request.ToName.Should().Be("Dave");
        request.OrderNumber.Should().Be("TICKET-002");
        request.Subject.Should().Be("Follow up");
        request.NewStatus.Should().Be("InProgress");
    }

    [Fact]
    public void SupportTicketUpdatedEmailRequest_EmailType_ReturnsSupportTicketUpdated()
    {
        var request = new SupportTicketUpdatedEmailRequest
        {
            ToEmail = "u@e.com",
            ToName = "U",
            OrderNumber = "O",
            Subject = "S",
            NewStatus = "N"
        };

        request.EmailType.Should().Be(EmailType.SupportTicketUpdated);
    }
}
