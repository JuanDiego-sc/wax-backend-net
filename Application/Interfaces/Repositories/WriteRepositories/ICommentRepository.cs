using Application.Interfaces.DTOs;
using Domain.Entities;

namespace Application.Interfaces.Repositories.WriteRepositories;

public interface ICommentRepository
{
    Task AddAsync(Comment comment, CancellationToken cancellationToken = default);
    Task<List<Comment>> GetByTicketIdAsync(string ticketId, CancellationToken cancellationToken = default);
}