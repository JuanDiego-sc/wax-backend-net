using Application.Interfaces;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Payment.Commands;
using Application.Payment.DTOs;
using UnitTests.Helpers.Fixtures;
using DomainBasket = Domain.Entities.Basket;

namespace UnitTests.Application.Payment;

public class CreateOrUpdateIntentCommandHandlerTests
{
    private readonly Mock<IBasketRepository> _basketRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPaymentService> _paymentService = new();
    private readonly CreateOrUpdateIntentCommandHandler _handler;

    public CreateOrUpdateIntentCommandHandlerTests()
    {
        _handler = new CreateOrUpdateIntentCommandHandler(
            _basketRepo.Object,
            _unitOfWork.Object,
            _paymentService.Object);
    }

    [Fact]
    public async Task Handle_WhenBasketNotFound_ReturnsFailure()
    {
        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainBasket?)null);

        var command = new CreateOrUpdateIntentCommand { BasketId = Guid.NewGuid().ToString() };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Basket not found");
    }

    [Fact]
    public async Task Handle_WhenPaymentIntentCreationFails_ReturnsFailure()
    {
        var basket = BasketFixtures.CreateBasketWithItems();
        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);

        _paymentService
            .Setup(p => p.CreateOrUpdatePaymentIntent(It.IsAny<DomainBasket>()))
            .Returns(Task.FromResult<PaymentIntentResult>(null!));

        var command = new CreateOrUpdateIntentCommand { BasketId = basket.BasketId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Problem creating payment intent");
    }

    [Fact]
    public async Task Handle_WhenBasketHasNoIntent_SetsNewIntentValues()
    {
        var basket = BasketFixtures.CreateBasketWithItems();
        var intentResult = new PaymentIntentResult
        {
            PaymentIntentId = "pi_123",
            ClientSecret = "cs_secret"
        };

        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);

        _paymentService
            .Setup(p => p.CreateOrUpdatePaymentIntent(It.IsAny<DomainBasket>()))
            .ReturnsAsync(intentResult);

        _unitOfWork.Setup(u => u.HasChanges()).Returns(true);
        _unitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var command = new CreateOrUpdateIntentCommand { BasketId = basket.BasketId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        basket.PaymentIntentId.Should().Be("pi_123");
        basket.ClientSecret.Should().Be("cs_secret");
    }

    [Fact]
    public async Task Handle_WhenBasketAlreadyHasIntent_DoesNotOverwrite()
    {
        var basket = BasketFixtures.CreateBasketWithItems(paymentIntentId: "existing_intent");
        basket.ClientSecret = "existing_secret";

        var intentResult = new PaymentIntentResult
        {
            PaymentIntentId = "new_intent",
            ClientSecret = "new_secret"
        };

        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);

        _paymentService
            .Setup(p => p.CreateOrUpdatePaymentIntent(It.IsAny<DomainBasket>()))
            .ReturnsAsync(intentResult);

        _unitOfWork.Setup(u => u.HasChanges()).Returns(false);

        var command = new CreateOrUpdateIntentCommand { BasketId = basket.BasketId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        basket.PaymentIntentId.Should().Be("existing_intent");
        basket.ClientSecret.Should().Be("existing_secret");
    }

    [Fact]
    public async Task Handle_WhenUnitOfWorkHasChangesAndSaveFails_ReturnsFailure()
    {
        var basket = BasketFixtures.CreateBasketWithItems();
        var intentResult = new PaymentIntentResult
        {
            PaymentIntentId = "pi_123",
            ClientSecret = "cs_secret"
        };

        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);

        _paymentService
            .Setup(p => p.CreateOrUpdatePaymentIntent(It.IsAny<DomainBasket>()))
            .ReturnsAsync(intentResult);

        _unitOfWork.Setup(u => u.HasChanges()).Returns(true);
        _unitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var command = new CreateOrUpdateIntentCommand { BasketId = basket.BasketId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Problem updating basket with intent");
    }

    [Fact]
    public async Task Handle_WhenNoChanges_DoesNotCallCompleteAsync()
    {
        var basket = BasketFixtures.CreateBasketWithItems(paymentIntentId: "existing_intent");
        basket.ClientSecret = "existing_secret";

        var intentResult = new PaymentIntentResult
        {
            PaymentIntentId = "new_intent",
            ClientSecret = "new_secret"
        };

        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);

        _paymentService
            .Setup(p => p.CreateOrUpdatePaymentIntent(It.IsAny<DomainBasket>()))
            .ReturnsAsync(intentResult);

        _unitOfWork.Setup(u => u.HasChanges()).Returns(false);

        var command = new CreateOrUpdateIntentCommand { BasketId = basket.BasketId };

        await _handler.Handle(command, CancellationToken.None);

        _unitOfWork.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSuccessful_ReturnsBasketDto()
    {
        var basket = BasketFixtures.CreateBasketWithItems();
        var intentResult = new PaymentIntentResult
        {
            PaymentIntentId = "pi_123",
            ClientSecret = "cs_secret"
        };

        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);

        _paymentService
            .Setup(p => p.CreateOrUpdatePaymentIntent(It.IsAny<DomainBasket>()))
            .ReturnsAsync(intentResult);

        _unitOfWork.Setup(u => u.HasChanges()).Returns(true);
        _unitOfWork.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var command = new CreateOrUpdateIntentCommand { BasketId = basket.BasketId };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.BasketId.Should().Be(basket.BasketId);
    }
}
