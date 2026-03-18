using Domain.SupportAssistAggregate;

namespace Application.Interfaces.Repositories.WriteRepositores;

public interface ISupportTicketRepository
{
    IQueryable<SupportTicket> GetQueryable();
    Task<SupportTicket?> GetTicketByIdAsync(string ticketId, CancellationToken cancellationToken);
    void Add(SupportTicket ticket);
    
}