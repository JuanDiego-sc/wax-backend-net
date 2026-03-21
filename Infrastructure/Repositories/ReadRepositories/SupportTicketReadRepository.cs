using System.Linq.Expressions;
using Application.Interfaces.Repositories.ReadRepositories;
using Application.SupportAssist.DTOs;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.ReadModels;

namespace Infrastructure.Repositories.ReadRepositories;

public class SupportTicketReadRepository(ReadDbContext context) : ISupportTicketReadRepository
{
    private static readonly Expression<Func<SupportTicketReadModel, SupportTicketDto>> MapToDto = t => new SupportTicketDto
    {
        Id = t.Id,
        UserId = t.UserId,
        UserEmail = t.UserEmail,
        UserFullName = t.UserFullName,
        OrderId = t.OrderId,
        Category = t.Category,
        Status = t.Status,
        Subject = t.Subject,
        Description = t.Description,
        CreatedAt = t.CreatedAt
    };

    public async Task<SupportTicketDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await context.SupportTickets
            .Where(t => t.Id == id)
            .Select(MapToDto)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public IQueryable<SupportTicketDto> GetSupportTickets()
    {
        return context.SupportTickets.Select(MapToDto);
    }
}
