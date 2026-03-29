namespace Application.Notifications.Requests;

public record AccountConfirmationEmailRequest : EmailRequest
{
    public override EmailType EmailType  => EmailType.AccountConfirmation;
    public required string ConfirmationLink { get; init; }
}