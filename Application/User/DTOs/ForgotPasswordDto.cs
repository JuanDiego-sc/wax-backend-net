namespace Application.User.DTOs;

public record ForgotPasswordDto
{
    public required string Email { get; set; }
}