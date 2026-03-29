namespace Application.Interfaces.DTOs;

public record CommentDto
{
    public required string Id { get; init; }
    public required string Body { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required string TicketId { get; set; }
    public required string UserId { get; init; }
    public required string UserName { get; init; }
}