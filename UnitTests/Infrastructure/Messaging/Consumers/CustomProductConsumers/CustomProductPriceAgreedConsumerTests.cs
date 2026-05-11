using Application.IntegrationEvents.CustomProductEvents;
using Application.Interfaces.Repositories.WriteRepositories;
using Domain.Entities;
using Domain.ProductAggregate;
using Infrastructure.Messaging.Consumers.CustomProductConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Infrastructure.Messaging.Consumers.CustomProductConsumers;

public class CustomProductPriceAgreedConsumerTests
{
    private readonly Mock<IBasketRepository> _basketRepo = new();
    private readonly Mock<ICustomProductRepository> _customProductRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private static ReadDbContext CreateReadContext() =>
        new(new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .Options);

    private CustomProductPriceAgreedConsumer BuildConsumer(ReadDbContext ctx) =>
        new(_basketRepo.Object, _customProductRepo.Object, _unitOfWork.Object, ctx,
            Mock.Of<ILogger<CustomProductPriceAgreedConsumer>>());

    private static ConsumeContext<CustomProductPriceAgreedIntegrationEvent> BuildContext(
        CustomProductPriceAgreedIntegrationEvent evt)
    {
        var mock = new Mock<ConsumeContext<CustomProductPriceAgreedIntegrationEvent>>();
        mock.Setup(c => c.Message).Returns(evt);
        mock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return mock.Object;
    }

    private static CustomProductPriceAgreedIntegrationEvent BuildEvent(
        string productId = "cp-1", string basketId = "basket-1", long agreedPrice = 4500) => new()
    {
        CustomProductId = productId,
        OwnerUserId = "user-1",
        BasketId = basketId,
        AgreedPrice = agreedPrice
    };

    private static void SeedReadModel(ReadDbContext ctx, string id, string status = "Approved")
    {
        ctx.CustomProducts.Add(new CustomProductReadModel
        {
            Id = id,
            Name = "Ring",
            Description = "Desc",
            Price = 5000,
            PictureUrl = "url",
            TaskId = Guid.NewGuid().ToString(),
            GlbUrl = "url",
            OwnerUserId = "user-1",
            Status = status,
            DesignType = "Ring",
            DesignMaterial = "Resin",
            DesignColor = "Blue",
            DesignShape = "Round",
            DesignDimensions = "5x5x5",
            CreatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();
        ctx.ChangeTracker.Clear();
    }

    [Fact]
    public async Task Consume_WhenProductNotFound_DoesNotThrow()
    {
        using var ctx = CreateReadContext();
        _customProductRepo
            .Setup(r => r.GetByIdAsync("missing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomProduct?)null);

        var consumer = BuildConsumer(ctx);

        var act = () => consumer.Consume(BuildContext(BuildEvent("missing")));

        await act.Should().NotThrowAsync();
        _unitOfWork.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_WhenAlreadyAddedToBasket_IsIdempotent()
    {
        using var ctx = CreateReadContext();
        var product = CustomProductFixtures.CreateCustomProduct("cp-1", status: CustomProductStatus.AddedToBasket);
        _customProductRepo
            .Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var consumer = BuildConsumer(ctx);
        await consumer.Consume(BuildContext(BuildEvent("cp-1")));

        _basketRepo.Verify(b => b.GetBasketWithItemsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_WhenBasketNotFound_CreatesBasketAndAddsItem()
    {
        using var ctx = CreateReadContext();
        var product = CustomProductFixtures.CreateCustomProduct("cp-1", status: CustomProductStatus.Approved);
        _customProductRepo
            .Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _basketRepo
            .Setup(b => b.GetBasketWithItemsAsync("basket-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Basket?)null);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var consumer = BuildConsumer(ctx);
        await consumer.Consume(BuildContext(BuildEvent("cp-1", "basket-1")));

        _basketRepo.Verify(b => b.Add(It.Is<Basket>(x => x.BasketId == "basket-1")), Times.Once);
        _unitOfWork.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        product.Status.Should().Be(CustomProductStatus.AddedToBasket);
    }

    [Fact]
    public async Task Consume_HappyPath_AddsToBasketAndUpdatesReadModel()
    {
        using var ctx = CreateReadContext();
        SeedReadModel(ctx, "cp-happy", "Approved");

        var product = CustomProductFixtures.CreateCustomProduct("cp-happy", status: CustomProductStatus.Approved);
        var basket = BasketFixtures.CreateBasket("basket-1");

        _customProductRepo
            .Setup(r => r.GetByIdAsync("cp-happy", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _basketRepo
            .Setup(b => b.GetBasketWithItemsAsync("basket-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var consumer = BuildConsumer(ctx);
        await consumer.Consume(BuildContext(BuildEvent("cp-happy", "basket-1", agreedPrice: 4500)));

        _unitOfWork.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
        product.Status.Should().Be(CustomProductStatus.AddedToBasket);
        basket.Items.Should().HaveCount(1);

        var readModel = await ctx.CustomProducts.FirstAsync(p => p.Id == "cp-happy");
        readModel.Status.Should().Be(CustomProductStatus.AddedToBasket.ToString());
        readModel.AgreedPrice.Should().Be(4500);
        readModel.Price.Should().Be(4500);
        readModel.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Consume_HappyPath_WhenNoReadModel_StillCompletesSuccessfully()
    {
        using var ctx = CreateReadContext();

        var product = CustomProductFixtures.CreateCustomProduct("cp-no-read", status: CustomProductStatus.Approved);
        var basket = BasketFixtures.CreateBasket("basket-1");

        _customProductRepo
            .Setup(r => r.GetByIdAsync("cp-no-read", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _basketRepo
            .Setup(b => b.GetBasketWithItemsAsync("basket-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(basket);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var consumer = BuildConsumer(ctx);

        var act = () => consumer.Consume(BuildContext(BuildEvent("cp-no-read", "basket-1")));

        await act.Should().NotThrowAsync();
        _unitOfWork.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
