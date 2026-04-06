namespace Application.User.DTOs;

public record ForgotPasswordRequest
{
    public required string Email { get; set; }
}