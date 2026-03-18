using Application.Core;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WriteRepositores;
using Application.SupportAssist.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.SupportAssist.Queries;

public class GetSupportTicketDetailsQueryHandler(
    ISupportTicketRepository supportTicketRepository)
    : IRequestHandler<GetSupportTicketDetailsQuery, Result<SupportTicketDto>>
{
    public async Task<Result<SupportTicketDto>> Handle(GetSupportTicketDetailsQuery request, CancellationToken cancellationToken)
    {
        var ticket = await supportTicketRepository
            .GetTicketByIdAsync(request.TicketId, cancellationToken);

        if (ticket == null) return Result<SupportTicketDto>.Failure("Ticket not found");

        return Result<SupportTicketDto>.Success(SupportTicketDto.FromEntity(ticket));

    }
}