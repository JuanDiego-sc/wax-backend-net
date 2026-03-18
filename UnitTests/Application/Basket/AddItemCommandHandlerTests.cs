using Application.Basket.Commands;
using Application.Basket.DTOs;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WriteRepositores;
using Moq;
using UnitTests.Helpers.Fixtures;
using FluentAssertions;

namespace UnitTests.Application.Basket;

public class AddItemCommandHandlerTests
{
    private readonly Mock<IBasketRepository> _basketRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly AddItemCommandHandler _handler;

    public AddItemCommandHandlerTests()
    {
        _handler = new AddItemCommandHandler(
            _basketRepo.Object,
            _productRepo.Object,
            _unitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsFailure()
    {
        var basket = BasketFixtures.CreateBasket();
        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        _productRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((global::Domain.Entities.Product?)null);

        var command = new AddItemCommand { BasketId = basket.BasketId, ProductId = "p1", Quantity = 1 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Product not found");
    }

    [Fact]
    public async Task Handle_WhenUnitOfWorkFails_ReturnsFailure()
    {
        var basket = BasketFixtures.CreateBasket();
        var product = ProductFixtures.CreateProduct();

        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new AddItemCommand { BasketId = basket.BasketId, ProductId = product.Id, Quantity = 1 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to add item to basket");
    }

    [Fact]
    public async Task Handle_WithExistingBasket_AddsItemAndReturnsSuccess()
    {
        var basket = BasketFixtures.CreateBasket();
        var product = ProductFixtures.CreateProduct();

        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(basket.BasketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new AddItemCommand { BasketId = basket.BasketId, ProductId = product.Id, Quantity = 2 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.BasketId.Should().Be(basket.BasketId);
    }

    [Fact]
    public async Task Handle_WhenBasketNotFound_CreatesNewBasketAndReturnsSuccess()
    {
        var product = ProductFixtures.CreateProduct();

        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((global::Domain.Entities.Basket?)null);
        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new AddItemCommand { BasketId = "new-basket", ProductId = product.Id, Quantity = 1 };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _basketRepo.Verify(r => r.Add(It.IsAny<global::Domain.Entities.Basket>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenSuccess_CallsUnitOfWorkCompleteAsync()
    {
        var basket = BasketFixtures.CreateBasket();
        var product = ProductFixtures.CreateProduct();

        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(basket.BasketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _handler.Handle(new AddItemCommand { BasketId = basket.BasketId, ProductId = product.Id, Quantity = 1 }, CancellationToken.None);

        _unitOfWork.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
