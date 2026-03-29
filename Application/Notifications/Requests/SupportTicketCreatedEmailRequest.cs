namespace Application.Notifications.Requests;

public record SupportTicketCreatedEmailRequest : EmailRequest
{
    public override EmailType EmailType => EmailType.SupportTicketCreated;
    public required string OrderNumber { get; set; }
    public required string Subject { get; init; }
}