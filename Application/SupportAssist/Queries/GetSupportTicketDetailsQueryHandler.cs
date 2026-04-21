using Application.Core;
using Application.Core.Validations;
using Application.Interfaces.Repositories.ReadRepositories;
using Application.SupportAssist.DTOs;
using MediatR;

namespace Application.SupportAssist.Queries;

public class GetSupportTicketDetailsQueryHandler(
    ISupportTicketReadRepository supportTicketReadRepository)
    : IRequestHandler<GetSupportTicketDetailsQuery, Result<SupportTicketDto>>
{
    public async Task<Result<SupportTicketDto>> Handle(GetSupportTicketDetailsQuery request, CancellationToken cancellationToken)
    {
        var ticket = await supportTicketReadRepository
            .GetByIdAsync(request.TicketId, cancellationToken);

        if (ticket == null) return Result<SupportTicketDto>.Failure("Ticket not found");

        return Result<SupportTicketDto>.Success(ticket);
    }
}