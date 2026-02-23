using System;
using System.ComponentModel.DataAnnotations;

namespace Application.User.DTOs;

public record RegisterDto
{
    [Required]
    public string Email { get; set; } = string.Empty;
    public required string Password { get; set; }
}
