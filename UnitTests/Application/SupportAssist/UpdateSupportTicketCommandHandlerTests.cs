using Application.IntegrationEvents.SupportTicketEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.SupportAssist.Commands;
using Application.SupportAssist.DTOs;
using Domain.SupportAssistAggregate;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.SupportAssist;

public class UpdateSupportTicketCommandHandlerTests
{
    private readonly Mock<ISupportTicketRepository> _ticketRepo = new();
    private readonly Mock<IEventPublisher> _eventPublisher = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly UpdateSupportTicketCommandHandler _handler;

    public UpdateSupportTicketCommandHandlerTests()
    {
        _handler = new UpdateSupportTicketCommandHandler(
            _ticketRepo.Object,
            _eventPublisher.Object,
            _unitOfWork.Object);
    }

    private static UpdateSupportTicketDto BuildUpdateDto(
        TicketCategory category = TicketCategory.PaymentIssue,
        TicketStatus status = TicketStatus.InProgress) => new()
    {
        Category = category,
        Status = status,
        Subject = "Updated Subject",
        Description = "Updated Description"
    };

    [Fact]
    public async Task Handle_WhenTicketNotFound_ReturnsFailure()
    {
        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupportTicket?)null);

        var command = new UpdateSupportTicketCommand
        {
            TicketId = Guid.NewGuid().ToString(),
            TicketDto = BuildUpdateDto()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Support ticket not found");
    }

    [Fact]
    public async Task Handle_WhenValidRequest_UpdatesTicketSuccessfully()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket(status: TicketStatus.Open);
        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var updateDto = BuildUpdateDto(status: TicketStatus.InProgress);
        var command = new UpdateSupportTicketCommand
        {
            TicketId = ticket.Id,
            TicketDto = updateDto
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        ticket.Subject.Should().Be("Updated Subject");
        ticket.Description.Should().Be("Updated Description");
        ticket.Status.Should().Be(TicketStatus.InProgress);
    }

    [Fact]
    public async Task Handle_WhenStatusChanges_PublishesStatusChangedEvent()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket(status: TicketStatus.Open);
        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var updateDto = BuildUpdateDto(status: TicketStatus.Closed);
        var command = new UpdateSupportTicketCommand
        {
            TicketId = ticket.Id,
            TicketDto = updateDto
        };

        await _handler.Handle(command, CancellationToken.None);

        _eventPublisher.Verify(
            e => e.PublishEventAsync(
                It.Is<SupportTicketStatusChangedIntegrationEvent>(ev => ev.NewStatus == "Closed"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenStatusRemainsSame_DoesNotPublishStatusChangedEvent()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket(status: TicketStatus.Open);
        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var updateDto = BuildUpdateDto(status: TicketStatus.Open);
        var command = new UpdateSupportTicketCommand
        {
            TicketId = ticket.Id,
            TicketDto = updateDto
        };

        await _handler.Handle(command, CancellationToken.None);

        _eventPublisher.Verify(
            e => e.PublishEventAsync(
                It.IsAny<SupportTicketStatusChangedIntegrationEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Always_PublishesUpdatedEvent()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket();
        _ticketRepo
            .Setup(r => r.GetTicketByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ticket);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new UpdateSupportTicketCommand
        {
            TicketId = ticket.Id,
            TicketDto = BuildUpdateDto()
        };

        await _handler.Handle(command, CancellationToken.None);

        _eventPublisher.Verify(
            e => e.PublishEventAsync(
                It.IsAny<SupportTicketUpdatedIntegrationEvent>(),
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

        var command = new UpdateSupportTicketCommand
        {
            TicketId = ticket.Id,
            TicketDto = BuildUpdateDto()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("An error occurred saving the data");
    }
}
