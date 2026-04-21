using Application.Core;
using Application.Core.Validations;
using Application.Interfaces.DTOs;
using MediatR;

namespace Application.Comments.Queries;

public class GetCommentsQuery : IRequest<Result<List<CommentDto>>>
{
    public required string TicketId { get; set; }
}