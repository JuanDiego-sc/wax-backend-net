using Application.Core;
using Application.Core.Pagination;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WriteRepositores;
using Application.SupportAssist.DTOs;
using Application.SupportAssist.Extensions;
using MediatR;

namespace Application.SupportAssist.Queries;

public class GetSupportTicketsQueryHandler(ISupportTicketRepository supportTicketRepository)
    : IRequestHandler<GetSupportTicketsQuery, Result<PagedList<SupportTicketDto>>>
{
    public async Task<Result<PagedList<SupportTicketDto>>> Handle
        (GetSupportTicketsQuery request, CancellationToken cancellationToken)
    {
        var ticketQuery = supportTicketRepository.GetQueryable()
            .Sort(request.TicketParams.OrderBy)
            .Filter(
                request.TicketParams.Status, 
                request.TicketParams.Category, 
                request.TicketParams.CreatedOn)
            .Select(x => x.ToDto());

        var tickets = await PagedList<SupportTicketDto>.ToPagedList(ticketQuery,
            request.TicketParams.PageNumber, request.TicketParams.PageSize);

        return Result<PagedList<SupportTicketDto>>.Success(tickets);
    }
}