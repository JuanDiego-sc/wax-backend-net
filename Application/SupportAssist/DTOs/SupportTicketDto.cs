using Domain.SupportAssistAggregate;

namespace Application.SupportAssist.DTOs;

public class SupportTicketDto
{
    public string Id { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    
    //public string? UserPhoneNumber { get; set; }


    public static SupportTicketDto FromEntity(SupportTicket ticket) => new()
    {
        Id = ticket.Id,
        UserId = ticket.UserId,
        OrderId = ticket.OrderId,
        Category = ticket.Category.ToString(),
        Status = ticket.Status.ToString(),
        Description = ticket.Description,
        Subject = ticket.Subject,
        CreatedAt = ticket.CreatedAt,
        UserEmail = ticket.User.UserName!,
        UserFullName = ticket.User.UserName!
    };
}