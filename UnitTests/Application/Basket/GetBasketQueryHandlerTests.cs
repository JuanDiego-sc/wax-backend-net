using Application.Basket.Queries;
using Application.Interfaces.Repositories;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Basket;

public class GetBasketQueryHandlerTests
{
    private readonly Mock<IBasketRepository> _basketRepo = new();
    private readonly GetBasketQueryHandler _handler;

    public GetBasketQueryHandlerTests()
    {
        _handler = new GetBasketQueryHandler(_basketRepo.Object);
    }

    [Fact]
    public async Task Handle_WhenBasketNotFound_ReturnsFailure()
    {
        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((global::Domain.Entities.Basket?)null);

        var result = await _handler.Handle(new GetBasketQuery { BasketId = "missing" }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Basket not found");
    }

    [Fact]
    public async Task Handle_WhenBasketFound_ReturnsMappedDto()
    {
        var basket = BasketFixtures.CreateBasketWithItems();
        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(basket.BasketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);

        var result = await _handler.Handle(new GetBasketQuery { BasketId = basket.BasketId }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.BasketId.Should().Be(basket.BasketId);
    }

    [Fact]
    public async Task Handle_WhenBasketHasItems_ReturnsDtoWithItems()
    {
        var product = ProductFixtures.CreateProduct();
        var item = BasketFixtures.CreateBasketItem(product.Id, quantity: 3);
        var basket = BasketFixtures.CreateBasketWithItems(items: [item]);

        _basketRepo
            .Setup(r => r.GetBasketWithItemsAsync(basket.BasketId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);

        var result = await _handler.Handle(new GetBasketQuery { BasketId = basket.BasketId }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items[0].Quantity.Should().Be(3);
    }
}
