using Application.Notifications;
using Application.Notifications.Requests;
using Infrastructure.Email.EmailTemplates;

namespace UnitTests.Infrastructure.Email;

// An unknown EmailRequest subclass that doesn't match any switch arm
file record UnknownEmailRequest : EmailRequest
{
    public override EmailType EmailType => (EmailType)999;
}

public class EmailTemplateServiceTests
{
    private readonly EmailTemplateService _service = new();

    [Fact]
    public void GetTemplate_ForAccountConfirmation_ReturnsSubjectAndBody()
    {
        var request = new AccountConfirmationEmailRequest
        {
            ToEmail = "user@example.com",
            ToName = "John",
            ConfirmationLink = "https://confirm"
        };

        var (subject, body) = _service.GetTemplate(request);

        subject.Should().NotBeNullOrEmpty();
        body.Should().NotBeNullOrEmpty();
        body.Should().Contain("John");
        body.Should().Contain("https://confirm");
    }

    [Fact]
    public void GetTemplate_ForPasswordReset_ReturnsSubjectAndBody()
    {
        var request = new PasswordResetEmailRequest
        {
            ToEmail = "user@example.com",
            ToName = "Alice",
            ResetCode = "CODE123",
            ResetLink = "https://reset"
        };

        var (subject, body) = _service.GetTemplate(request);

        subject.Should().NotBeNullOrEmpty();
        body.Should().NotBeNullOrEmpty();
        body.Should().Contain("CODE123");
        body.Should().Contain("https://reset");
    }

    [Fact]
    public void GetTemplate_ForOrderStatusChanged_ReturnsSubjectAndBody()
    {
        var request = new OrderStatusChangedEmailRequest
        {
            ToEmail = "user@example.com",
            ToName = "Bob",
            OrderNumber = "ORDER-12345678",
            OldStatus = "Pending",
            NewStatus = "Approved",
            Total = 10000
        };

        var (subject, body) = _service.GetTemplate(request);

        subject.Should().NotBeNullOrEmpty();
        body.Should().NotBeNullOrEmpty();
        body.Should().Contain("Pending");
        body.Should().Contain("Approved");
    }

    [Fact]
    public void GetTemplate_ForPaymentConfirmed_ReturnsSubjectAndBody()
    {
        var request = new PaymentConfirmedEmailRequest
        {
            ToEmail = "user@example.com",
            ToName = "Carol",
            OrderNumber = "ORDER-12345678",
            TotalAmount = 25000
        };

        var (subject, body) = _service.GetTemplate(request);

        subject.Should().NotBeNullOrEmpty();
        body.Should().NotBeNullOrEmpty();
        body.Should().Contain("Carol");
    }

    [Fact]
    public void GetTemplate_ForSupportTicketCreated_ReturnsSubjectAndBody()
    {
        var request = new SupportTicketCreatedEmailRequest
        {
            ToEmail = "user@example.com",
            ToName = "Dave",
            OrderNumber = "TICKET-12345678",
            Subject = "Product is broken"
        };

        var (subject, body) = _service.GetTemplate(request);

        subject.Should().NotBeNullOrEmpty();
        body.Should().NotBeNullOrEmpty();
        body.Should().Contain("Product is broken");
        body.Should().Contain("Dave");
    }

    [Fact]
    public void GetTemplate_ForSupportTicketUpdated_ReturnsSubjectAndBody()
    {
        var request = new SupportTicketUpdatedEmailRequest
        {
            ToEmail = "user@example.com",
            ToName = "Eve",
            OrderNumber = "TICKET-12345678",
            Subject = "Follow up issue",
            NewStatus = "InProgress"
        };

        var (subject, body) = _service.GetTemplate(request);

        subject.Should().NotBeNullOrEmpty();
        body.Should().NotBeNullOrEmpty();
        body.Should().Contain("InProgress");
        body.Should().Contain("Follow up issue");
    }

    [Fact]
    public void GetTemplate_ForUnknownEmailType_ThrowsArgumentException()
    {
        var request = new UnknownEmailRequest
        {
            ToEmail = "user@example.com",
            ToName = "Unknown"
        };

        var act = () => _service.GetTemplate(request);

        act.Should().Throw<ArgumentException>();
    }
}
