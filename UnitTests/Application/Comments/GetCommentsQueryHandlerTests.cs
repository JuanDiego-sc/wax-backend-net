using Application.Comments.Queries;
using Application.Interfaces.Repositories.WriteRepositories;
using Domain.Entities;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Comments;

public class GetCommentsQueryHandlerTests
{
    private readonly Mock<ICommentRepository> _commentRepo = new();
    private readonly GetCommentsQueryHandler _handler;

    public GetCommentsQueryHandlerTests()
    {
        _handler = new GetCommentsQueryHandler(_commentRepo.Object);
    }

    private static Comment CreateComment(string? ticketId = null, string body = "Test comment")
    {
        var user = SupportTicketFixtures.CreateUser();
        return new Comment
        {
            Id = Guid.NewGuid().ToString(),
            Body = body,
            TicketId = ticketId ?? Guid.NewGuid().ToString(),
            UserId = user.Id,
            User = user,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Handle_WhenNoComments_ReturnsEmptyList()
    {
        _commentRepo
            .Setup(r => r.GetByTicketIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>());

        var query = new GetCommentsQuery { TicketId = Guid.NewGuid().ToString() };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCommentsExist_ReturnsCommentDtos()
    {
        var ticketId = Guid.NewGuid().ToString();
        var comments = new List<Comment>
        {
            CreateComment(ticketId, "First comment"),
            CreateComment(ticketId, "Second comment")
        };

        _commentRepo
            .Setup(r => r.GetByTicketIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(comments);

        var query = new GetCommentsQuery { TicketId = ticketId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value![0].Body.Should().Be("First comment");
        result.Value[1].Body.Should().Be("Second comment");
    }

    [Fact]
    public async Task Handle_MapsAllFieldsCorrectly()
    {
        var ticketId = Guid.NewGuid().ToString();
        var comment = CreateComment(ticketId, "Test body");

        _commentRepo
            .Setup(r => r.GetByTicketIdAsync(ticketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment> { comment });

        var query = new GetCommentsQuery { TicketId = ticketId };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var dto = result.Value!.First();
        dto.Id.Should().Be(comment.Id);
        dto.Body.Should().Be(comment.Body);
        dto.TicketId.Should().Be(comment.TicketId);
        dto.UserId.Should().Be(comment.UserId);
        dto.UserName.Should().Be(comment.User.UserName);
    }

    [Fact]
    public async Task Handle_AlwaysReturnsSuccess()
    {
        _commentRepo
            .Setup(r => r.GetByTicketIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Comment>());

        var query = new GetCommentsQuery { TicketId = "any-id" };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }
}
