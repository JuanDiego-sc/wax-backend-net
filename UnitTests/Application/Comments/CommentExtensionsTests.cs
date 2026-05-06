using Application.Comments.Extensions;
using Application.Interfaces.DTOs;
using Domain.Entities;
using UserEntity = Domain.Entities.User;

namespace UnitTests.Application.Comments;

public class CommentExtensionsTests
{
    private static Comment BuildComment(string? userName = "testuser")
    {
        var user = new UserEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserName = userName
        };

        return new Comment
        {
            Id = "comment-1",
            Body = "Test body",
            CreatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            TicketId = "ticket-1",
            UserId = user.Id,
            User = user
        };
    }

    [Fact]
    public void ToDto_MapsAllFields()
    {
        var comment = BuildComment("testuser");

        var dto = comment.ToDto();

        dto.Id.Should().Be(comment.Id);
        dto.Body.Should().Be(comment.Body);
        dto.CreatedAt.Should().Be(comment.CreatedAt);
        dto.TicketId.Should().Be(comment.TicketId);
        dto.UserId.Should().Be(comment.UserId);
        dto.UserName.Should().Be("testuser");
    }

    [Fact]
    public void ToDto_WhenUserNameIsNull_UsesEmptyString()
    {
        var comment = BuildComment(null);

        var dto = comment.ToDto();

        dto.UserName.Should().Be(string.Empty);
    }

    [Fact]
    public void ProjectToDto_ProjectsAllFields()
    {
        var comment = BuildComment("projecteduser");
        var queryable = new List<Comment> { comment }.AsQueryable();

        var result = queryable.ProjectToDto().ToList();

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(comment.Id);
        result[0].Body.Should().Be(comment.Body);
        result[0].CreatedAt.Should().Be(comment.CreatedAt);
        result[0].TicketId.Should().Be(comment.TicketId);
        result[0].UserId.Should().Be(comment.UserId);
        result[0].UserName.Should().Be("projecteduser");
    }

    [Fact]
    public void ProjectToDto_WhenUserNameIsNull_UsesEmptyString()
    {
        var comment = BuildComment(null);
        var queryable = new List<Comment> { comment }.AsQueryable();

        var result = queryable.ProjectToDto().ToList();

        result.Should().HaveCount(1);
        result[0].UserName.Should().Be(string.Empty);
    }

    [Fact]
    public void ProjectToDto_WithMultipleComments_ProjectsAll()
    {
        var comments = new List<Comment>
        {
            BuildComment("user1"),
            BuildComment("user2")
        };
        var queryable = comments.AsQueryable();

        var result = queryable.ProjectToDto().ToList();

        result.Should().HaveCount(2);
        result[0].UserName.Should().Be("user1");
        result[1].UserName.Should().Be("user2");
    }
}
