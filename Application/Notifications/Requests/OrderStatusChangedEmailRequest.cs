namespace Application.Notifications.Requests;

public record OrderStatusChangedEmailRequest : EmailRequest
{
    public override EmailType EmailType => EmailType.OrderStatusChanged;
    public required string OrderNumber { get; init; }
    public required string OldStatus { get; init; }
    public required string NewStatus { get; init; }
    public int Total { get; init; }
}