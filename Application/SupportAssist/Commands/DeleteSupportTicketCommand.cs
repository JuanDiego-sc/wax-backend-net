using Application.Core;
using Application.Core.Validations;
using MediatR;

namespace Application.SupportAssist.Commands;

public class DeleteSupportTicketCommand : IRequest<Result<Unit>>
{
    public required string TicketId { get; init; }
}
