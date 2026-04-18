using Application.Core;
using Application.Core.Validations;
using Application.SupportAssist.DTOs;
using MediatR;

namespace Application.SupportAssist.Commands;

public class UpdateSupportTicketCommand : IRequest<Result<Unit>>
{
    public required string TicketId { get; init; }
    public required UpdateSupportTicketDto TicketDto { get; init; }
}
