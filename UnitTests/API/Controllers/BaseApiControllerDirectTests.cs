using API.Controllers;
using Application.Core.Pagination;
using Application.Core.Validations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UnitTests.Helpers;

namespace UnitTests.API.Controllers;

public class BaseApiControllerDirectTests
{
    private class TestPagedQuery : IRequest<Result<PagedList<string>>> { }
    private class TestInfinityQuery : IRequest<Result<InfinityPagedList<string, string>>> { }

    private class TestController : BaseApiController
    {
        public Task<ActionResult> CallHandlePagedQuery<T>(IRequest<Result<PagedList<T>>> query)
            => HandlePagedQuery(query);

        public Task<ActionResult> CallHandleInfinityPagedQuery<T, TCursor>(
            IRequest<Result<InfinityPagedList<T, TCursor>>> query)
            => HandleInfinityPagedQuery(query);
    }

    private readonly Mock<IMediator> _mediator;
    private readonly TestController _controller;

    public BaseApiControllerDirectTests()
    {
        (_mediator, _controller) = ControllerTestFactory.Create(new TestController());
    }

    // ── HandlePagedQuery ──────────────────────────────────────────────────────

    [Fact]
    public async Task HandlePagedQuery_WhenFailureIsNotNotFound_ReturnsBadRequest()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<TestPagedQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<string>>.Failure("Validation error"));

        var result = await _controller.CallHandlePagedQuery(new TestPagedQuery());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── HandleInfinityPagedQuery ──────────────────────────────────────────────

    [Fact]
    public async Task HandleInfinityPagedQuery_WhenSuccess_ReturnsOkAndAppendsNextCursorHeader()
    {
        var list = new InfinityPagedList<string, string>
        {
            Items = ["item1"],
            NextCursor = "cursor-abc"
        };

        _mediator
            .Setup(m => m.Send(It.IsAny<TestInfinityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InfinityPagedList<string, string>>.Success(list));

        var result = await _controller.CallHandleInfinityPagedQuery(new TestInfinityQuery());

        result.Should().BeOfType<OkObjectResult>();
        _controller.Response.Headers.ContainsKey("NextCursor").Should().BeTrue();
    }

    [Fact]
    public async Task HandleInfinityPagedQuery_WhenResultIs404_ReturnsNotFound()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<TestInfinityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InfinityPagedList<string, string>>.Failure("Not found", 404));

        var result = await _controller.CallHandleInfinityPagedQuery(new TestInfinityQuery());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task HandleInfinityPagedQuery_WhenFailureIsNotNotFound_ReturnsBadRequest()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<TestInfinityQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<InfinityPagedList<string, string>>.Failure("Bad input"));

        var result = await _controller.CallHandleInfinityPagedQuery(new TestInfinityQuery());

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
