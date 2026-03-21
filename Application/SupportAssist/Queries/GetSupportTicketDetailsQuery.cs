using Application.Core;
using Application.SupportAssist.DTOs;
using MediatR;

namespace Application.SupportAssist.Queries;

public class GetSupportTicketDetailsQuery : IRequest<Result<SupportTicketDto>>
{
    public required string TicketId { get; set; }
}