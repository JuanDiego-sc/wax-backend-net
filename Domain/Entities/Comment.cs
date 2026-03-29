using Domain.SupportAssistAggregate;

namespace Domain.Entities;

public class Comment : BaseEntity
{
    public required string Body { get; set; }
    
    //Navigation props
    public required string TicketId { get; set; }
    public SupportTicket SupportTicket { get; set; } = null!;

    public required string UserId { get; set; }
    public User User { get; set; } = null!;
}