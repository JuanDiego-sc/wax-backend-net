using Domain.SupportAssistAggregate;

namespace Application.SupportAssist.DTOs;

public record UpdateSupportTicketDto
{
    public TicketCategory Category { get; init; }
    public TicketStatus Status { get; init; }
    public required string Subject { get; init; }
    public required string Description { get; init; }

    public void ApplyTo(SupportTicket ticket)
    {
        ticket.Category = Category;
        ticket.Status = Status;
        ticket.Subject = Subject;
        ticket.Description = Description;
    }
}
