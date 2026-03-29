using Application.Interfaces.DTOs;
using Domain.Entities;

namespace Application.Comments.Extensions;

public static class CommentExtensions
{
    public static IQueryable<CommentDto> ProjectToDto(this IQueryable<Comment> query)
    {
        return query.Select(comment => new CommentDto
        {
            Id = comment.Id,
            Body = comment.Body,
            CreatedAt = comment.CreatedAt,
            TicketId = comment.TicketId,
            UserId = comment.UserId,
            UserName = comment.User.UserName ?? ""
        });
    }
    public static CommentDto ToDto(this Comment comment)
    {
        return new CommentDto
        {
            Id = comment.Id,
            Body = comment.Body,
            CreatedAt = comment.CreatedAt,
            TicketId = comment.TicketId,
            UserId = comment.UserId,
            UserName = comment.User.UserName ?? ""
        };
    }
}