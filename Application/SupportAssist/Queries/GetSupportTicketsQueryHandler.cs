using Application.Core;
using Application.Core.Pagination;
using Application.Core.Validations;
using Application.Interfaces.Repositories.ReadRepositories;
using Application.SupportAssist.DTOs;
using Application.SupportAssist.Extensions;
using MediatR;

namespace Application.SupportAssist.Queries;

public class GetSupportTicketsQueryHandler(ISupportTicketReadRepository supportTicketReadRepository)
    : IRequestHandler<GetSupportTicketsQuery, Result<PagedList<SupportTicketDto>>>
{
    public async Task<Result<PagedList<SupportTicketDto>>> Handle
        (GetSupportTicketsQuery request, CancellationToken cancellationToken)
    {
        var ticketQuery = supportTicketReadRepository.GetSupportTickets()
            .Sort(request.TicketParams.OrderBy)
            .Filter(
                request.TicketParams.Status, 
                request.TicketParams.Category, 
                request.TicketParams.CreatedOn);

        var tickets = await PagedList<SupportTicketDto>.ToPagedList(ticketQuery,
            request.TicketParams.PageNumber, request.TicketParams.PageSize);

        return Result<PagedList<SupportTicketDto>>.Success(tickets);
    }
}