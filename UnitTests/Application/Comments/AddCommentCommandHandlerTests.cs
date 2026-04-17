using Application.Comments.Commands;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Domain.SupportAssistAggregate;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Comments;

public class AddCommentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ISupportTicketRepository> _ticketRepo = new();
    private readonly Mock<IUserAccessor> _userAccessor = new();
    private readonly AddCommentCommandHandler _handler;

    public AddCommentCommandHandlerTests()
    {
        _handler = new AddCommentCommandHandler(
            _unitOfWork.Object,
            _ticketRepo.Object,
            _userAccessor.Object);
    }

    [Fact]
    public async Task Handle_WhenTicketNotFound_ReturnsFailure()
    {
        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportTicket?)null);

        var command = new AddCommentCommand
        {
            Body = "Test comment",
            TicketId = Guid.NewGuid().ToString()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Could not find the ticket");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsFailure()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket();
        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _userAccessor
            .Setup(u => u.GetUserAsync())
            .ReturnsAsync((global::Domain.Entities.User?)null);

        var command = new AddCommentCommand
        {
            Body = "Test comment",
            TicketId = ticket.Id
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Could not find the user");
    }

    [Fact]
    public async Task Handle_WhenValidRequest_AddsCommentToTicket()
    {
        var user = SupportTicketFixtures.CreateUser();
        var ticket = SupportTicketFixtures.CreateSupportTicket(user: user);
        ticket.Comments = new List<Comment>();

        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _userAccessor
            .Setup(u => u.GetUserAsync())
            .ReturnsAsync(user);

        Comment? capturedComment = null;
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                if (ticket.Comments.Count > 0)
                {
                    capturedComment = ticket.Comments.First();
                    capturedComment.User = user;
                }
            })
            .ReturnsAsync(true);

        var command = new AddCommentCommand
        {
            Body = "Test comment body",
            TicketId = ticket.Id
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        ticket.Comments.Should().HaveCount(1);
        ticket.Comments.First().Body.Should().Be("Test comment body");
        ticket.Comments.First().UserId.Should().Be(user.Id);
    }

    [Fact]
    public async Task Handle_WhenSaveFails_ReturnsFailure()
    {
        var user = SupportTicketFixtures.CreateUser();
        var ticket = SupportTicketFixtures.CreateSupportTicket(user: user);
        ticket.Comments = new List<Comment>();

        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _userAccessor
            .Setup(u => u.GetUserAsync())
            .ReturnsAsync(user);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new AddCommentCommand
        {
            Body = "Test comment",
            TicketId = ticket.Id
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Could not delete the comment");
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ReturnsCommentDto()
    {
        var user = SupportTicketFixtures.CreateUser();
        var ticket = SupportTicketFixtures.CreateSupportTicket(user: user);
        ticket.Comments = new List<Comment>();

        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _userAccessor
            .Setup(u => u.GetUserAsync())
            .ReturnsAsync(user);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .Callback(() =>
            {
                if (ticket.Comments.Count > 0)
                {
                    ticket.Comments.First().User = user;
                }
            })
            .ReturnsAsync(true);

        var command = new AddCommentCommand
        {
            Body = "My comment",
            TicketId = ticket.Id
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Body.Should().Be("My comment");
        result.Value.UserId.Should().Be(user.Id);
        result.Value.TicketId.Should().Be(ticket.Id);
    }
}
