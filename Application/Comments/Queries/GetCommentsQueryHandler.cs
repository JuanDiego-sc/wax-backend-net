using Application.Comments.Extensions;
using Application.Core;
using Application.Core.Validations;
using Application.Interfaces.DTOs;
using Application.Interfaces.Repositories.WriteRepositories;
using MediatR;

namespace Application.Comments.Queries;

public class GetCommentsQueryHandler(ICommentRepository commentRepository)
    : IRequestHandler<GetCommentsQuery, Result<List<CommentDto>>>
{
    public async Task<Result<List<CommentDto>>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        var comments = await commentRepository.GetByTicketIdAsync(request.TicketId, cancellationToken);
        
        return Result<List<CommentDto>>.Success(comments.Select(x => x.ToDto()).ToList());
    }
}