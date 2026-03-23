using Application.Interfaces;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Orders.Commands;
using Application.Orders.DTOs;
using Domain.Entities;
using Domain.OrderAggregate;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Orders;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IBasketRepository> _basketRepo = new();
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IUserAccessor> _userAccessor = new();
    private readonly Mock<IEventPublisher> _eventPublisher = new();
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _handler = new CreateOrderCommandHandler(
            _basketRepo.Object,
            _orderRepo.Object,
            _unitOfWork.Object,
            _userAccessor.Object,
            _eventPublisher.Object);
    }

    private static CreateOrderDto BuildOrderDto() => new()
    {
        BillingAddress = OrderFixtures.CreateBillingAddress(),
        PaymentSummary = OrderFixtures.CreatePaymentSummary()
    };

    [Fact]
    public async Task Handle_WhenBasketNotFound_ReturnsFailure()
    {
        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((global::Domain.Entities.Basket?)null);

        _userAccessor.Setup(u => u.GetUserAsync())
            .ReturnsAsync(new User { Email = "user@test.com", UserName = "user" });

        var command = new CreateOrderCommand { BasketId = "b1", OrderDto = BuildOrderDto() };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Basket not found or is empty");
    }

    [Fact]
    public async Task Handle_WhenBasketHasNoPaymentIntent_ReturnsFailure()
    {
        var basket = BasketFixtures.CreateBasketWithItems(paymentIntentId: null);
        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(basket.BasketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        _userAccessor.Setup(u => u.GetUserAsync())
            .ReturnsAsync(new User { Email = "user@test.com", UserName = "user" });

        var command = new CreateOrderCommand { BasketId = basket.BasketId, OrderDto = BuildOrderDto() };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Basket not found or is empty");
    }

    [Fact]
    public async Task Handle_WhenItemOutOfStock_ReturnsFailure()
    {
        var product = ProductFixtures.CreateProduct(quantityInStock: 0);
        var item = BasketFixtures.CreateBasketItem(product.Id, quantity: 5);
        item.Product.QuantityInStock = 0;
        var basket = BasketFixtures.CreateBasketWithItems(
            paymentIntentId: "pi_test",
            items: [item]);

        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(basket.BasketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        _userAccessor.Setup(u => u.GetUserAsync())
            .ReturnsAsync(new User { Email = "user@test.com", UserName = "user" });

        var command = new CreateOrderCommand { BasketId = basket.BasketId, OrderDto = BuildOrderDto() };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("One or more items in the basket are out of stock");
    }

    [Fact]
    public async Task Handle_WhenOrderNotExisting_CreatesNewOrder()
    {
        var basket = BasketFixtures.CreateBasketWithItems(paymentIntentId: "pi_new");
        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(basket.BasketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        _userAccessor.Setup(u => u.GetUserAsync())
            .ReturnsAsync(new User { Email = "buyer@test.com", UserName = "buyer", Address = OrderFixtures.CreateBillingAddress(), AddressId = "addr-1" });
        _orderRepo
            .Setup(r => r.GetByPaymentIntentIdAsync("pi_new", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateOrderCommand { BasketId = basket.BasketId, OrderDto = BuildOrderDto() };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _orderRepo.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOrderAlreadyExists_UpdatesExistingOrder()
    {
        var basket = BasketFixtures.CreateBasketWithItems(paymentIntentId: "pi_existing");
        var existingOrder = OrderFixtures.CreateOrder(paymentIntentId: "pi_existing");

        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(basket.BasketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        _userAccessor.Setup(u => u.GetUserAsync())
            .ReturnsAsync(new User { Email = "buyer@test.com", UserName = "buyer" });
        _orderRepo
            .Setup(r => r.GetByPaymentIntentIdAsync("pi_existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingOrder);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateOrderCommand { BasketId = basket.BasketId, OrderDto = BuildOrderDto() };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _orderRepo.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SubtotalOver10000_SetsDeliveryFeeToZero()
    {
        var product = ProductFixtures.CreateProduct(price: 15000, quantityInStock: 10);
        var item = BasketFixtures.CreateBasketItem(quantity: 1, product: product);
        var basket = BasketFixtures.CreateBasketWithItems(paymentIntentId: "pi_free", items: [item]);

        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(basket.BasketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        _userAccessor.Setup(u => u.GetUserAsync())
            .ReturnsAsync(new User { Email = "buyer@test.com", UserName = "buyer", Address = OrderFixtures.CreateBillingAddress(), AddressId = "addr-1" });
        _orderRepo
            .Setup(r => r.GetByPaymentIntentIdAsync("pi_free", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Order? capturedOrder = null;
        _orderRepo.Setup(r => r.Add(It.IsAny<Order>()))
            .Callback<Order>(o => capturedOrder = o);

        var command = new CreateOrderCommand { BasketId = basket.BasketId, OrderDto = BuildOrderDto() };
        await _handler.Handle(command, CancellationToken.None);

        capturedOrder!.DeliveryFee.Should().Be(0);
    }
}
