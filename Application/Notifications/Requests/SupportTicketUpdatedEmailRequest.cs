namespace Application.Notifications.Requests;

public record SupportTicketUpdatedEmailRequest : EmailRequest
{
    public override EmailType EmailType => EmailType.SupportTicketUpdated;
    public required string OrderNumber { get; init; }
    public required string Subject { get; init; }
    public required string NewStatus { get; init; }
}