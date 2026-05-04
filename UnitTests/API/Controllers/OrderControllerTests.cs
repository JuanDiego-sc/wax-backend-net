using API.Controllers;
using Application.Basket.Interfaces;
using Application.Core;
using Application.Core.Pagination;
using Application.Core.Validations;
using Application.Orders.Commands;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using Application.Orders.Queries;
using Domain.OrderAggregate;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UnitTests.Helpers;

namespace UnitTests.API.Controllers;

public class OrderControllerTests
{
    private readonly Mock<IMediator> _mediator;
    private readonly Mock<IBasketProvider> _basketProvider;
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        _basketProvider = new Mock<IBasketProvider>();
        (_mediator, _controller) = ControllerTestFactory.Create(new OrderController(_basketProvider.Object));
    }

    // ── GetOrders ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrders_DelegatesToMediatorWithCorrectParams()
    {
        var orderParams = new OrderParams { PageNumber = 1, PageSize = 10 };
        var pagedList = new PagedList<OrderDto>(new List<OrderDto>(), 0, 1, 10);

        _mediator
            .Setup(m => m.Send(It.Is<GetOrdersQuery>(q => q.OrderParams == orderParams), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<OrderDto>>.Success(pagedList));

        await _controller.GetOrders(orderParams);

        _mediator.Verify(
            m => m.Send(It.Is<GetOrdersQuery>(q => q.OrderParams == orderParams), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrders_ReturnsPaginationHeader_WhenSuccess()
    {
        var orderParams = new OrderParams { PageNumber = 1, PageSize = 10 };
        var pagedList = new PagedList<OrderDto>(new List<OrderDto>(), 0, 1, 10);

        _mediator
            .Setup(m => m.Send(It.IsAny<GetOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<OrderDto>>.Success(pagedList));

        var result = await _controller.GetOrders(orderParams);

        result.Result.Should().BeOfType<OkObjectResult>();
        _controller.Response.Headers.ContainsKey("Pagination").Should().BeTrue();
    }

    [Fact]
    public async Task GetOrders_ReturnsNotFound_WhenResultIs404()
    {
        var orderParams = new OrderParams();

        _mediator
            .Setup(m => m.Send(It.IsAny<GetOrdersQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<OrderDto>>.Failure("Not found", 404));

        var result = await _controller.GetOrders(orderParams);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ── GetOrderDetails ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrderDetails_DelegatesToMediatorWithCorrectId()
    {
        const string orderId = "order-abc";
        var dto = BuildOrderDto(orderId);

        _mediator
            .Setup(m => m.Send(It.Is<GetOrderDetailsQuery>(q => q.OrderId == orderId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDto>.Success(dto));

        await _controller.GetOrderDetails(orderId);

        _mediator.Verify(
            m => m.Send(It.Is<GetOrderDetailsQuery>(q => q.OrderId == orderId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOrderDetails_ReturnsOk_WhenSuccess()
    {
        const string orderId = "order-ok";
        var dto = BuildOrderDto(orderId);

        _mediator
            .Setup(m => m.Send(It.IsAny<GetOrderDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDto>.Success(dto));

        var result = await _controller.GetOrderDetails(orderId);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetOrderDetails_ReturnsNotFound_WhenResultIs404()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<GetOrderDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDto>.Failure("Order not found", 404));

        var result = await _controller.GetOrderDetails("missing");

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ── CreateOrder ───────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrder_UsesBasketProviderAndSendsCommand()
    {
        const string basketId = "basket-xyz";
        var orderDto = BuildCreateOrderDto();
        var returnedOrder = BuildOrderDto("order-new");

        _basketProvider.Setup(p => p.GetBasketId()).Returns(basketId);
        _mediator
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDto>.Success(returnedOrder));

        await _controller.CreateOrder(orderDto);

        _mediator.Verify(
            m => m.Send(
                It.Is<CreateOrderCommand>(c => c.BasketId == basketId && c.OrderDto == orderDto),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrder_WithNullBasketId_UsesEmptyString()
    {
        var orderDto = BuildCreateOrderDto();
        var returnedOrder = BuildOrderDto("order-empty-basket");

        _basketProvider.Setup(p => p.GetBasketId()).Returns((string?)null);
        _mediator
            .Setup(m => m.Send(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<OrderDto>.Success(returnedOrder));

        await _controller.CreateOrder(orderDto);

        _mediator.Verify(
            m => m.Send(
                It.Is<CreateOrderCommand>(c => c.BasketId == string.Empty),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static OrderDto BuildOrderDto(string id) => new()
    {
        Id = id,
        BuyerEmail = "buyer@example.com",
        OrderStatus = "Pending",
        Subtotal = 1000,
        DeliveryFee = 100,
        Total = 1100,
        CreatedAt = DateTime.UtcNow,
        BillingAddress = new BillingAddressDto
        {
            Name = "Test User",
            Line1 = "Calle 1",
            City = "Quito",
            Country = "EC",
            State = "Calle 2",
            PostalCode = "12345"
        },
        PaymentSummary = new PaymentSummaryDto
        {
            Last4 = 4242,
            Brand = "visa",
            ExpMonth = 12,
            ExpYear = 2026
        }
    };

    private static CreateOrderDto BuildCreateOrderDto() => new()
    {
        PaymentSummary = new PaymentSummary
        {
            Last4 = 4242,
            Brand = "visa",
            ExpMonth = 12,
            ExpYear = 2026
        }
    };
}
