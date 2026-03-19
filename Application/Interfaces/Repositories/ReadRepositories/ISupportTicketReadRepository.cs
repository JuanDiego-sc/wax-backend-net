using Application.SupportAssist.DTOs;

namespace Application.Interfaces.Repositories.ReadRepositories;

public interface ISupportTicketReadRepository
{
    Task<SupportTicketDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    IQueryable<SupportTicketDto> GetSupportTickets();
}
