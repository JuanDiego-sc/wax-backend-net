namespace Application.User.DTOs;

public record RegisterDto
{
    public required string Email { get; init; } = string.Empty;
    public required string Password { get; set; }
}
