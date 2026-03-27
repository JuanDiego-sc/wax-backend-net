namespace Application.Notifications.Requests;

public record PaymentConfirmedEmailRequest : EmailRequest
{
    public override EmailType EmailType => EmailType.PaymentCompleted;
    public required string OrderNumber { get; init; }
    public required long TotalAmount { get; init; }
}