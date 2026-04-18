using Application.Core;
using Application.Core.Validations;
using Application.SupportAssist.DTOs;
using MediatR;

namespace Application.SupportAssist.Commands;

public class CreateSupportTicketCommand : IRequest<Result<string>>
{
    public required CreateSupportTicketDto TicketDto { get; set; }
}