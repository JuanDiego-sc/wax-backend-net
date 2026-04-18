using Application.Core;
using Application.Core.Pagination;
using Application.Core.Validations;
using Application.SupportAssist.DTOs;
using Application.SupportAssist.Extensions;
using MediatR;

namespace Application.SupportAssist.Queries;

public class GetSupportTicketsQuery : IRequest<Result<PagedList<SupportTicketDto>>>
{
    public required SupportTicketParams TicketParams { get; set; }
}