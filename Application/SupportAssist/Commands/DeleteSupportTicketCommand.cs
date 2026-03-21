using Application.Core;
using MediatR;

namespace Application.SupportAssist.Commands;

public class DeleteSupportTicketCommand : IRequest<Result<Unit>>
{
    public required string TicketId { get; init; }
}
