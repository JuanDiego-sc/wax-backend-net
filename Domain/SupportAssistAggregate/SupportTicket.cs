using Domain.Entities;
using Domain.OrderAggregate;

namespace Domain.SupportAssistAggregate;

public class SupportTicket : BaseEntity
{
    public TicketCategory Category { get; set; }
    public TicketStatus Status { get; set; }
    public required string Subject { get; set; }
    public required string Description { get; set; }

    #region Navigation properties

    public string UserId { get; set; } = "";
    public User User { get; set; } = null!;
    
    public string OrderId { get; set; } = "";
    public Order Order { get; set; } = null!;
    
    #endregion
}