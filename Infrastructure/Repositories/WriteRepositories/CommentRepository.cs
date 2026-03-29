using Persistence;
using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Repositories.WriteRepositories;
using Domain.Entities;

namespace Infrastructure.Repositories.WriteRepositories;

public class CommentRepository(WriteDbContext context) : ICommentRepository
{
    public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        await context.Comments.AddAsync(comment, cancellationToken);
    }

    public async Task<List<Comment>> GetByTicketIdAsync(string ticketId, CancellationToken cancellationToken = default)
    {
        return await context.Comments
            .Where(x => x.TicketId == ticketId)
            .OrderByDescending(x => x.CreatedAt)
            .Include(x => x.User)
            .ToListAsync(cancellationToken);
    }
}