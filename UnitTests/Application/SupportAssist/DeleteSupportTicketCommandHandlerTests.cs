using Application.IntegrationEvents.SupportTicketEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.SupportAssist.Commands;
using Domain.SupportAssistAggregate;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.SupportAssist;

public class DeleteSupportTicketCommandHandlerTests
{
    private readonly Mock<ISupportTicketRepository> _ticketRepo = new();
    private readonly Mock<IEventPublisher> _eventPublisher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly DeleteSupportTicketCommandHandler _handler;

    public DeleteSupportTicketCommandHandlerTests()
    {
        _handler = new DeleteSupportTicketCommandHandler(
            _ticketRepo.Object,
            _eventPublisher.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenTicketNotFound_ReturnsFailure()
    {
        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportTicket?)null);

        var command = new DeleteSupportTicketCommand { TicketId = Guid.NewGuid().ToString() };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Support ticket not found");
    }

    [Fact]
    public async Task Handle_WhenTicketExists_RemovesTicket()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket();
        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new DeleteSupportTicketCommand { TicketId = ticket.Id };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _ticketRepo.Verify(r => r.Remove(ticket), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTicketDeleted_PublishesDeletedEvent()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket();
        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new DeleteSupportTicketCommand { TicketId = ticket.Id };

        await _handler.Handle(command, CancellationToken.None);

        _eventPublisher.Verify(
            e => e.PublishEventAsync(
                It.Is<SupportTicketDeletedIntegrationEvent>(ev => ev.TicketId == ticket.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSaveFails_ReturnsFailure()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket();
        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new DeleteSupportTicketCommand { TicketId = ticket.Id };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("An error occurred deleting the data");
    }

    [Fact]
    public async Task Handle_WhenSaveSucceeds_ReturnsUnitValue()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket();
        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new DeleteSupportTicketCommand { TicketId = ticket.Id };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(MediatR.Unit.Value);
    }
}
