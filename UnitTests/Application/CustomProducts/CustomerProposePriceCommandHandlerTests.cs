using Application.CustomProducts.Commands;
using Application.CustomProducts.DTOs;
using Application.IntegrationEvents.CustomProductEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using Domain.ProductAggregate;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.CustomProducts;

public class CustomerProposePriceCommandHandlerTests
{
    private readonly Mock<ICustomProductRepository> _repo = new();
    private readonly Mock<IEventPublisher> _events = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly CustomerProposePriceCommandHandler _handler;

    public CustomerProposePriceCommandHandlerTests()
    {
        _handler = new CustomerProposePriceCommandHandler(_repo.Object, _events.Object, _uow.Object);
        _uow.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((CustomProduct?)null);

        var result = await _handler.Handle(new CustomerProposePriceCommand
        {
            CustomProductId = "x",
            OwnerUserId = "user-1",
            ProposeCustomPrice = new ProposeCustomPriceDto { Amount = 4000 }
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenOwnerMismatch_ReturnsFailure()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1", ownerUserId: "other")
            .WithSystemQuotation().WithAdminProposal();
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _handler.Handle(new CustomerProposePriceCommand
        {
            CustomProductId = "cp-1",
            OwnerUserId = "user-1",
            ProposeCustomPrice = new ProposeCustomPriceDto { Amount = 4000 }
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("owner");
    }

    [Fact]
    public async Task Handle_WhenBasketIdProvided_StoresItOnProduct()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1").WithSystemQuotation().WithAdminProposal();
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await _handler.Handle(new CustomerProposePriceCommand
        {
            CustomProductId = "cp-1",
            OwnerUserId = "user-1",
            BasketId = "basket-xyz",
            ProposeCustomPrice = new ProposeCustomPriceDto { Amount = 3500 }
        }, CancellationToken.None);

        product.BasketId.Should().Be("basket-xyz");
    }

    [Fact]
    public async Task Handle_WhenBasketIdEmpty_DoesNotOverwriteExistingBasketId()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1", basketId: "existing-basket")
            .WithSystemQuotation().WithAdminProposal();
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await _handler.Handle(new CustomerProposePriceCommand
        {
            CustomProductId = "cp-1",
            OwnerUserId = "user-1",
            BasketId = null,
            ProposeCustomPrice = new ProposeCustomPriceDto { Amount = 3500 }
        }, CancellationToken.None);

        product.BasketId.Should().Be("existing-basket");
    }

    [Fact]
    public async Task Handle_HappyPath_RegistersProposalAndPublishesEvent()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1").WithSystemQuotation().WithAdminProposal();
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _handler.Handle(new CustomerProposePriceCommand
        {
            CustomProductId = "cp-1",
            OwnerUserId = "user-1",
            ProposeCustomPrice = new ProposeCustomPriceDto { Amount = 3800, Comment = "lower please" }
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(CustomProductStatus.AwaitingAdminReview);
        product.Proposals.Should().HaveCount(3);
        _events.Verify(e => e.PublishEventAsync(
            It.IsAny<CustomProductPriceUpdatedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
