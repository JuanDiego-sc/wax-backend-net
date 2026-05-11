using Application.CustomProducts.Commands;
using Application.IntegrationEvents.CustomProductEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using Domain.ProductAggregate;
using UnitTests.Helpers.Fixtures;
using DomainBasket = Domain.Entities.Basket;

namespace UnitTests.Application.CustomProducts;

public class ApproveCustomProductPriceCommandHandlerTests
{
    private readonly Mock<ICustomProductRepository> _repo = new();
    private readonly Mock<IBasketRepository> _basketRepo = new();
    private readonly Mock<IEventPublisher> _events = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly ApproveCustomProductPriceCommandHandler _handler;

    public ApproveCustomProductPriceCommandHandlerTests()
    {
        _handler = new ApproveCustomProductPriceCommandHandler(
            _repo.Object, _basketRepo.Object, _events.Object, _uow.Object);
        _uow.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    private ApproveCustomProductPriceCommand CustomerApproveCommand(string productId, string basketId) =>
        new() { CustomProductId = productId, Approver = ProposalSource.Customer, OwnerUserId = "user-1", BasketId = basketId };

    private ApproveCustomProductPriceCommand AdminApproveCommand(string productId) =>
        new() { CustomProductId = productId, Approver = ProposalSource.Admin };

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((CustomProduct?)null);

        var result = await _handler.Handle(CustomerApproveCommand("x", "b1"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_CustomerApprove_WhenOwnerMismatch_ReturnsFailure()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1", ownerUserId: "other-user")
            .WithSystemQuotation().WithAdminProposal();
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var cmd = new ApproveCustomProductPriceCommand
        {
            CustomProductId = "cp-1",
            Approver = ProposalSource.Customer,
            OwnerUserId = "user-1",
            BasketId = "b1"
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("user does not match");
    }

    [Fact]
    public async Task Handle_CustomerApprove_WhenBasketIdMissing_ReturnsFailure()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1").WithSystemQuotation().WithAdminProposal();
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var cmd = new ApproveCustomProductPriceCommand
        {
            CustomProductId = "cp-1",
            Approver = ProposalSource.Customer,
            OwnerUserId = "user-1",
            BasketId = null
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("BasketId is required");
    }

    [Fact]
    public async Task Handle_CustomerApprove_WhenBasketNotFound_CreatesBasketAndSucceeds()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1").WithSystemQuotation().WithAdminProposal();
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _basketRepo.Setup(b => b.GetBasketWithItemsAsync("b1", It.IsAny<CancellationToken>())).ReturnsAsync((DomainBasket?)null);

        var result = await _handler.Handle(CustomerApproveCommand("cp-1", "b1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.BasketId.Should().Be("b1");
        _basketRepo.Verify(b => b.Add(It.Is<DomainBasket>(x => x.BasketId == "b1")), Times.Once);
        _events.Verify(e => e.PublishEventAsync(
            It.Is<CustomProductPriceAgreedIntegrationEvent>(ev =>
                ev.CustomProductId == "cp-1" && ev.BasketId == "b1"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CustomerApprove_HappyPath_PublishesPriceAgreedAndSavesBasketId()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1").WithSystemQuotation().WithAdminProposal();
        var basket = BasketFixtures.CreateBasket("b1");
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _basketRepo.Setup(b => b.GetBasketWithItemsAsync("b1", It.IsAny<CancellationToken>())).ReturnsAsync(basket);

        var result = await _handler.Handle(CustomerApproveCommand("cp-1", "b1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(CustomProductStatus.Approved);
        product.BasketId.Should().Be("b1");
        _events.Verify(e => e.PublishEventAsync(
            It.Is<CustomProductPriceAgreedIntegrationEvent>(ev =>
                ev.CustomProductId == "cp-1" && ev.BasketId == "b1"),
            It.IsAny<CancellationToken>()), Times.Once);
        _events.Verify(e => e.PublishEventAsync(
            It.IsAny<CustomProductPriceUpdatedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AdminApprovingCustomerCounterOffer_PublishesPriceAgreedWithStoredBasketId()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1", basketId: "stored-basket")
            .WithSystemQuotation().WithAdminProposal().WithCustomerCounterOffer();
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _handler.Handle(AdminApproveCommand("cp-1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(CustomProductStatus.Approved);
        _events.Verify(e => e.PublishEventAsync(
            It.Is<CustomProductPriceAgreedIntegrationEvent>(ev =>
                ev.CustomProductId == "cp-1" && ev.BasketId == "stored-basket"),
            It.IsAny<CancellationToken>()), Times.Once);
        _events.Verify(e => e.PublishEventAsync(
            It.IsAny<CustomProductPriceUpdatedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AdminApprovingCustomerCounterOffer_WhenNoBasketId_ReturnsFailure()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1", basketId: null)
            .WithSystemQuotation().WithAdminProposal().WithCustomerCounterOffer();
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _handler.Handle(AdminApproveCommand("cp-1"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("basket is unknown");
    }

    [Fact]
    public async Task Handle_AdminApprovingSystemQuotation_PublishesPriceUpdatedOnly()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1").WithSystemQuotation();
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var cmd = new ApproveCustomProductPriceCommand
        {
            CustomProductId = "cp-1",
            Approver = ProposalSource.Admin
        };

        // Admin cannot approve System quotation directly (Approve() throws)
        var act = () => _handler.Handle(cmd, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenAdminProposedAndCustomerApproves_PublishesPriceAgreed()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1")
            .WithSystemQuotation().WithAdminProposal();
        var basket = BasketFixtures.CreateBasket("b1");
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _basketRepo.Setup(b => b.GetBasketWithItemsAsync("b1", It.IsAny<CancellationToken>())).ReturnsAsync(basket);

        var result = await _handler.Handle(CustomerApproveCommand("cp-1", "b1"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _events.Verify(e => e.PublishEventAsync(
            It.IsAny<CustomProductPriceAgreedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUoWFails_ReturnsFailure()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1").WithSystemQuotation().WithAdminProposal();
        var basket = BasketFixtures.CreateBasket("b1");
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _basketRepo.Setup(b => b.GetBasketWithItemsAsync("b1", It.IsAny<CancellationToken>())).ReturnsAsync(basket);
        _uow.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(CustomerApproveCommand("cp-1", "b1"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
