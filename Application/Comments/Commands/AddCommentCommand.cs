using Application.Core;
using Application.Interfaces.DTOs;
using MediatR;

namespace Application.Comments.Commands;

public class AddCommentCommand : IRequest<Result<CommentDto>>
{
    public required string Body { get; set; }
    public required string TicketId { get; set; }
}