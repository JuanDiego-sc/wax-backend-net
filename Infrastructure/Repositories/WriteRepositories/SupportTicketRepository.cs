using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WriteRepositores;
using Domain.SupportAssistAggregate;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories;

public class SupportTicketRepository(WriteDbContext context) : ISupportTicketRepository
{
    public IQueryable<SupportTicket> GetQueryable()
    {
        return context.SupportTickets
            .Include(x => x.User)
            .AsQueryable();
    }

    public async Task<SupportTicket?> GetTicketByIdAsync(string ticketId, CancellationToken cancellationToken)
    {
        return await context.SupportTickets
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == ticketId, cancellationToken);
    }

    public void Add(SupportTicket ticket)
    {
        context.SupportTickets.Add(ticket);
    }
    
}