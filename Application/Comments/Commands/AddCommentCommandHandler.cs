using Application.Comments.Extensions;
using Application.Core;
using Application.Interfaces;
using Application.Interfaces.DTOs;
using Application.Interfaces.Repositories.WriteRepositories;
using Domain.Entities;
using MediatR;

namespace Application.Comments.Commands;

public class AddCommentCommandHandler(
    IUnitOfWork unitOfWork,
    ISupportTicketRepository supportTicketRepository,
    IUserAccessor userAccessor) : IRequestHandler<AddCommentCommand, Result<CommentDto>>
{
    public async Task<Result<CommentDto>> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var ticket = await supportTicketRepository.GetTicketByIdAsync(request.TicketId, cancellationToken);
        if (ticket == null) return Result<CommentDto>.Failure("Could not find the ticket");

        var user = await userAccessor.GetUserAsync();
        if(user == null) return Result<CommentDto>.Failure("Could not find the user");

        var comment = new Comment
        {
            Body = request.Body,
            UserId = user.Id,
            TicketId = request.TicketId,
        };
        
        ticket.Comments.Add(comment);
        
        var result = await unitOfWork.CompleteAsync(cancellationToken);
        
        return !result 
            ?  Result<CommentDto>.Failure("Could not delete the comment")
            : Result<CommentDto>.Success(comment.ToDto());

    }
}