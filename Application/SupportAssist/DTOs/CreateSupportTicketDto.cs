using Domain.SupportAssistAggregate;

namespace Application.SupportAssist.DTOs;

public record CreateSupportTicketDto
{
    public required string OrderId { get; set; }
    public TicketCategory TicketCategory { get; set; }
    public TicketStatus Status { get; set; }
    public required string Subject { get; set; }
    public required string Description { get; set; }
}