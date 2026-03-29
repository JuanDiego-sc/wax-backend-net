namespace Application.User.DTOs;

public record UserDto
{
    public required string Id { get; set; }
    public required string Email { get; set; }
    public required string UserName { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? Role { get; set; }
}