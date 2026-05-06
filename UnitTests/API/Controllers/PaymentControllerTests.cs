using API.Controllers;
using Application.Basket.DTOs;
using Application.Basket.Interfaces;
using Application.Core;
using Application.Core.Validations;
using Application.Payment.Commands;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnitTests.Helpers;

namespace UnitTests.API.Controllers;

public class PaymentControllerTests
{
    private readonly Mock<IBasketProvider> _basketProvider = new();
    private readonly Mock<IMediator> _mediator;
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        (_mediator, _controller) = ControllerTestFactory.Create(new PaymentController(_basketProvider.Object));
    }

    // ── CreateOrUpdateIntent ──────────────────────────────────────────────────

    [Fact]
    public async Task CreateOrUpdateIntent_UsesBasketProviderAndSendsCommand()
    {
        const string basketId = "basket-abc";
        _basketProvider.Setup(p => p.GetBasketId()).Returns(basketId);
        _mediator
            .Setup(m => m.Send(It.IsAny<CreateOrUpdateIntentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<BasketDto>.Success(BuildBasketDto(basketId)));

        await _controller.CreateOrUpdateIntent();

        _mediator.Verify(
            m => m.Send(
                It.Is<CreateOrUpdateIntentCommand>(c => c.BasketId == basketId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrUpdateIntent_WithNullBasketId_UsesEmptyString()
    {
        _basketProvider.Setup(p => p.GetBasketId()).Returns((string?)null);
        _mediator
            .Setup(m => m.Send(It.IsAny<CreateOrUpdateIntentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<BasketDto>.Success(BuildBasketDto(string.Empty)));

        await _controller.CreateOrUpdateIntent();

        _mediator.Verify(
            m => m.Send(
                It.Is<CreateOrUpdateIntentCommand>(c => c.BasketId == string.Empty),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrUpdateIntent_ReturnsOk_WhenSuccess()
    {
        const string basketId = "basket-ok";
        var dto = BuildBasketDto(basketId);
        _basketProvider.Setup(p => p.GetBasketId()).Returns(basketId);
        _mediator
            .Setup(m => m.Send(It.IsAny<CreateOrUpdateIntentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<BasketDto>.Success(dto));

        var result = await _controller.CreateOrUpdateIntent();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task CreateOrUpdateIntent_ReturnsBadRequest_WhenFailure()
    {
        _basketProvider.Setup(p => p.GetBasketId()).Returns("basket-fail");
        _mediator
            .Setup(m => m.Send(It.IsAny<CreateOrUpdateIntentCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<BasketDto>.Failure("Payment service unavailable"));

        var result = await _controller.CreateOrUpdateIntent();

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── StripeWebhook ─────────────────────────────────────────────────────────

    [Fact]
    public async Task StripeWebhook_SendsCommandWithSignatureAndPayload()
    {
        const string signature = "whsec_test_sig";
        const string payload = "{\"type\":\"payment_intent.succeeded\"}";

        _controller.ControllerContext.HttpContext.Request.Body =
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(payload));

        _mediator
            .Setup(m => m.Send(It.IsAny<HandleStripeWebhookCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));

        await _controller.StripeWebhook(signature);

        _mediator.Verify(
            m => m.Send(
                It.Is<HandleStripeWebhookCommand>(c =>
                    c.Signature == signature &&
                    c.Payload == payload),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task StripeWebhook_WithNullSignature_UsesEmptyString()
    {
        const string payload = "{}";
        _controller.ControllerContext.HttpContext.Request.Body =
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(payload));

        _mediator
            .Setup(m => m.Send(It.IsAny<HandleStripeWebhookCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));

        await _controller.StripeWebhook(null);

        _mediator.Verify(
            m => m.Send(
                It.Is<HandleStripeWebhookCommand>(c => c.Signature == string.Empty),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static BasketDto BuildBasketDto(string basketId) => new()
    {
        BasketId = basketId,
        Items = [],
        ClientSecret = "cs_test_secret",
        PaymentIntentId = "pi_test_123"
    };
}
