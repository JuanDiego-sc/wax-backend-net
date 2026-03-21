using Domain.SupportAssistAggregate;

namespace Application.SupportAssist.DTOs;

public record CreateSupportTicketDto
{
    public required string OrderId { get; set; }
    public TicketCategory Category { get; set; }
    public TicketStatus Status { get; set; }
    public required string Subject { get; set; }
    public required string Description { get; set; }


    public SupportTicket ToEntity(string userId) => new()
    {
        UserId = userId,
        OrderId = OrderId,
        Category = Category,
        Status = Status,
        Description = Description,
        Subject = Subject
    };
}