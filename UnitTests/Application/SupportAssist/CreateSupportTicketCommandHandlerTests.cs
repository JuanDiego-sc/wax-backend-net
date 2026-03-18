using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WriteRepositores;
using Application.SupportAssist.Commands;
using Application.SupportAssist.DTOs;
using Domain.Entities;
using Domain.OrderAggregate;
using Domain.SupportAssistAggregate;
using UnitTests.Helpers.Fixtures;


namespace UnitTests.Application.SupportAssist;

public class CreateSupportTicketCommandHandlerTests
{
    private readonly Mock<ISupportTicketRepository> _ticketRepo = new();
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IUserAccessor> _userAccessor = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly CreateSupportTicketCommandHandler _handler;

    public CreateSupportTicketCommandHandlerTests()
    {
        _handler = new CreateSupportTicketCommandHandler(
            _ticketRepo.Object,
            _orderRepo.Object,
            _userAccessor.Object,
            _unitOfWork.Object);
    }

    private static CreateSupportTicketDto BuildTicketDto(string? orderId = null) => new()
    {
        OrderId = orderId ?? Guid.NewGuid().ToString(),
        Category = TicketCategory.Other,
        Status = TicketStatus.Open,
        Subject = "Test Subject",
        Description = "Test Description"
    };

    [Fact]
    public async Task Handle_WhenOrderNotFound_ReturnsFailure()
    {
        _orderRepo
            .Setup(r => r.GetByOrderIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        _userAccessor.Setup(u => u.GetUserAsync())
            .ReturnsAsync(SupportTicketFixtures.CreateUser());

        var command = new CreateSupportTicketCommand { TicketDto = BuildTicketDto() };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Order not found");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ReturnsFailure()
    {
        var order = OrderFixtures.CreateOrder();
        _orderRepo
            .Setup(r => r.GetByOrderIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _userAccessor.Setup(u => u.GetUserAsync())
            .ReturnsAsync((User?)null);

        var command = new CreateSupportTicketCommand { TicketDto = BuildTicketDto() };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");
    }

    [Fact]
    public async Task Handle_WhenValidRequest_CreatesTicketSuccessfully()
    {
        var user = SupportTicketFixtures.CreateUser();
        var order = OrderFixtures.CreateOrder();

        _orderRepo
            .Setup(r => r.GetByOrderIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _userAccessor.Setup(u => u.GetUserAsync())
            .ReturnsAsync(user);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        SupportTicket? capturedTicket = null;
        _ticketRepo.Setup(r => r.Add(It.IsAny<SupportTicket>()))
            .Callback<SupportTicket>(t => capturedTicket = t);

        var command = new CreateSupportTicketCommand { TicketDto = BuildTicketDto(order.Id) };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _ticketRepo.Verify(r => r.Add(It.IsAny<SupportTicket>()), Times.Once);
        capturedTicket.Should().NotBeNull();
        capturedTicket!.UserId.Should().Be(user.Id);
        capturedTicket.OrderId.Should().Be(order.Id);
    }

    [Fact]
    public async Task Handle_WhenSaveFails_ReturnsFailure()
    {
        var user = SupportTicketFixtures.CreateUser();
        var order = OrderFixtures.CreateOrder();

        _orderRepo
            .Setup(r => r.GetByOrderIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _userAccessor.Setup(u => u.GetUserAsync())
            .ReturnsAsync(user);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateSupportTicketCommand { TicketDto = BuildTicketDto() };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("An error occured saving the data");
    }

    [Fact]
    public async Task Handle_WhenValidRequest_ReturnsTicketId()
    {
        var user = SupportTicketFixtures.CreateUser();
        var order = OrderFixtures.CreateOrder();

        _orderRepo
            .Setup(r => r.GetByOrderIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _userAccessor.Setup(u => u.GetUserAsync())
            .ReturnsAsync(user);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        SupportTicket? capturedTicket = null;
        _ticketRepo.Setup(r => r.Add(It.IsAny<SupportTicket>()))
            .Callback<SupportTicket>(t => capturedTicket = t);

        var command = new CreateSupportTicketCommand { TicketDto = BuildTicketDto() };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(capturedTicket!.Id);
    }
}
