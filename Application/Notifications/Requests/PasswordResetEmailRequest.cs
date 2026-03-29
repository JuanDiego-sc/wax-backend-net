namespace Application.Notifications.Requests;

public record PasswordResetEmailRequest : EmailRequest
{
    public override EmailType EmailType => EmailType.PasswordReset;
    public required string ResetLink { get; init; }
    public required string ResetCode { get; init; }
}