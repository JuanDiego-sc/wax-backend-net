using API.Controllers;
using Application.Core.Pagination;
using Application.Core.Validations;
using Application.SupportAssist.Commands;
using Application.SupportAssist.DTOs;
using Application.SupportAssist.Extensions;
using Application.SupportAssist.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UnitTests.Helpers;

namespace UnitTests.API.Controllers;

public class SupportControllerTests
{
    private readonly Mock<IMediator> _mediator;
    private readonly SupportController _controller;

    public SupportControllerTests()
    {
        (_mediator, _controller) = ControllerTestFactory.Create(new SupportController());
    }

    [Fact]
    public async Task GetSupportTickets_DelegatesToMediatorWithCorrectParams()
    {
        var ticketParams = new SupportTicketParams { PageNumber = 1, PageSize = 10, Status = "Open" };
        var pagedList = new PagedList<SupportTicketDto>(new List<SupportTicketDto>(), 0, 1, 10);

        GetSupportTicketsQuery? capturedQuery = null;
        _mediator
            .Setup(m => m.Send(It.IsAny<GetSupportTicketsQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<PagedList<SupportTicketDto>>>, CancellationToken>((q, _) => capturedQuery = (GetSupportTicketsQuery)q)
            .ReturnsAsync(Result<PagedList<SupportTicketDto>>.Success(pagedList));

        await _controller.GetSupportTickets(ticketParams);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.TicketParams.Should().BeSameAs(ticketParams);
    }

    [Fact]
    public async Task GetSupportTickets_ReturnsPaginationHeader_WhenSuccess()
    {
        var ticketParams = new SupportTicketParams { PageNumber = 1, PageSize = 5 };
        var pagedList = new PagedList<SupportTicketDto>(new List<SupportTicketDto>(), 0, 1, 5);

        _mediator
            .Setup(m => m.Send(It.IsAny<GetSupportTicketsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<SupportTicketDto>>.Success(pagedList));

        var result = await _controller.GetSupportTickets(ticketParams);

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(pagedList);
        _controller.Response.Headers.ContainsKey("Pagination").Should().BeTrue();
    }

    [Fact]
    public async Task GetSupportTickets_ReturnsNotFound_WhenResultIs404()
    {
        var ticketParams = new SupportTicketParams { PageNumber = 1, PageSize = 10 };

        _mediator
            .Setup(m => m.Send(It.IsAny<GetSupportTicketsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<SupportTicketDto>>.Failure("Not found", 404));

        var result = await _controller.GetSupportTickets(ticketParams);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetSupportTicket_DelegatesToMediatorWithCorrectId()
    {
        const string ticketId = "ticket-abc";
        var ticketDto = new SupportTicketDto { Id = ticketId };

        GetSupportTicketDetailsQuery? capturedQuery = null;
        _mediator
            .Setup(m => m.Send(It.IsAny<GetSupportTicketDetailsQuery>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<SupportTicketDto>>, CancellationToken>((q, _) => capturedQuery = (GetSupportTicketDetailsQuery)q)
            .ReturnsAsync(Result<SupportTicketDto>.Success(ticketDto));

        await _controller.GetSupportTicket(ticketId);

        capturedQuery.Should().NotBeNull();
        capturedQuery!.TicketId.Should().Be(ticketId);
    }

    [Fact]
    public async Task GetSupportTicket_ReturnsOk_WhenSuccess()
    {
        const string ticketId = "ticket-abc";
        var ticketDto = new SupportTicketDto { Id = ticketId };

        _mediator
            .Setup(m => m.Send(It.IsAny<GetSupportTicketDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SupportTicketDto>.Success(ticketDto));

        var result = await _controller.GetSupportTicket(ticketId);

        var okResult = result.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(ticketDto);
    }

    [Fact]
    public async Task GetSupportTicket_ReturnsNotFound_WhenResultIs404()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<GetSupportTicketDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<SupportTicketDto>.Failure("Ticket not found", 404));

        var result = await _controller.GetSupportTicket("non-existent-id");

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateSupportTicket_DelegatesToCommand_WithCorrectDto()
    {
        var createDto = new CreateSupportTicketDto
        {
            OrderId = "order-1",
            Subject = "My issue",
            Description = "Details here"
        };

        CreateSupportTicketCommand? capturedCommand = null;
        _mediator
            .Setup(m => m.Send(It.IsAny<CreateSupportTicketCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<string>>, CancellationToken>((c, _) => capturedCommand = (CreateSupportTicketCommand)c)
            .ReturnsAsync(Result<string>.Success("ticket-new-id"));

        await _controller.CreateSupportTicket(createDto);

        capturedCommand.Should().NotBeNull();
        capturedCommand!.TicketDto.Should().BeSameAs(createDto);
    }

    [Fact]
    public async Task UpdateSupportTicket_DelegatesToCommand_WithCorrectIdAndDto()
    {
        const string ticketId = "ticket-xyz";
        var updateDto = new UpdateSupportTicketDto { Subject = "Updated", Description = "Updated description" };

        UpdateSupportTicketCommand? capturedCommand = null;
        _mediator
            .Setup(m => m.Send(It.IsAny<UpdateSupportTicketCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<Unit>>, CancellationToken>((c, _) => capturedCommand = (UpdateSupportTicketCommand)c)
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));

        await _controller.UpdateSupportTicket(ticketId, updateDto);

        capturedCommand.Should().NotBeNull();
        capturedCommand!.TicketId.Should().Be(ticketId);
        capturedCommand.TicketDto.Should().BeSameAs(updateDto);
    }

    [Fact]
    public async Task DeleteSupportTicket_DelegatesToCommand_WithCorrectId()
    {
        const string ticketId = "ticket-to-delete";

        DeleteSupportTicketCommand? capturedCommand = null;
        _mediator
            .Setup(m => m.Send(It.IsAny<DeleteSupportTicketCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<Unit>>, CancellationToken>((c, _) => capturedCommand = (DeleteSupportTicketCommand)c)
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));

        await _controller.DeleteSupportTicket(ticketId);

        capturedCommand.Should().NotBeNull();
        capturedCommand!.TicketId.Should().Be(ticketId);
    }

    [Fact]
    public async Task DeleteSupportTicket_ReturnsNotFound_WhenResultIs404()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<DeleteSupportTicketCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Failure("Ticket not found", 404));

        var result = await _controller.DeleteSupportTicket("non-existent-id");

        result.Should().BeOfType<NotFoundResult>();
    }
}
