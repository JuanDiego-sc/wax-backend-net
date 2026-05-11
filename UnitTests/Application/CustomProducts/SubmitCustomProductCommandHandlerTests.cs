using Application.CustomProducts.Commands;
using Application.IntegrationEvents.CustomProductEvents;
using Application.Interfaces.DTOs;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Interfaces.Services;
using Domain.ProductAggregate;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.CustomProducts;

public class SubmitCustomProductCommandHandlerTests
{
    private readonly Mock<ICustomProductRepository> _repo = new();
    private readonly Mock<IQuotationService> _quotation = new();
    private readonly Mock<IEventPublisher> _events = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly SubmitCustomProductCommandHandler _handler;

    public SubmitCustomProductCommandHandlerTests()
    {
        _handler = new SubmitCustomProductCommandHandler(
            _repo.Object, _quotation.Object, _events.Object, _uow.Object);
        _uow.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _quotation
            .Setup(q => q.QuoteAsync(It.IsAny<CustomProductDesign>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuotationResult(5000, new Dictionary<string, decimal>()));
    }

    [Fact]
    public async Task Handle_WhenTaskIdAlreadyExists_ReturnsFailure()
    {
        var existing = CustomProductFixtures.CreateCustomProduct("cp-1");
        _repo.Setup(r => r.GetByTaskIdAsync("task-dup", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var result = await _handler.Handle(new SubmitCustomProductCommand
        {
            CustomProduct = CustomProductFixtures.BuildSubmitRequest("task-dup"),
            OwnerUserId = "user-1"
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Already exists");
    }

    [Fact]
    public async Task Handle_HappyPath_CreatesProductWithSystemQuotation()
    {
        _repo.Setup(r => r.GetByTaskIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomProduct?)null);

        CustomProduct? captured = null;
        _repo.Setup(r => r.Add(It.IsAny<CustomProduct>()))
            .Callback<CustomProduct>(p => captured = p);

        var result = await _handler.Handle(new SubmitCustomProductCommand
        {
            CustomProduct = CustomProductFixtures.BuildSubmitRequest(),
            OwnerUserId = "user-1"
        }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Status.Should().Be(CustomProductStatus.AwaitingAdminReview);
        captured.Proposals.Should().HaveCount(1);
        captured.Proposals[0].Source.Should().Be(ProposalSource.System);
        captured.Price.Should().Be(5000);
    }

    [Fact]
    public async Task Handle_WhenBasketIdProvided_StoresItOnProduct()
    {
        _repo.Setup(r => r.GetByTaskIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomProduct?)null);

        CustomProduct? captured = null;
        _repo.Setup(r => r.Add(It.IsAny<CustomProduct>()))
            .Callback<CustomProduct>(p => captured = p);

        await _handler.Handle(new SubmitCustomProductCommand
        {
            CustomProduct = CustomProductFixtures.BuildSubmitRequest(),
            OwnerUserId = "user-1",
            BasketId = "cookie-basket"
        }, CancellationToken.None);

        captured!.BasketId.Should().Be("cookie-basket");
    }

    [Fact]
    public async Task Handle_HappyPath_PublishesSubmittedEvent()
    {
        _repo.Setup(r => r.GetByTaskIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomProduct?)null);
        _repo.Setup(r => r.Add(It.IsAny<CustomProduct>()));

        await _handler.Handle(new SubmitCustomProductCommand
        {
            CustomProduct = CustomProductFixtures.BuildSubmitRequest(),
            OwnerUserId = "user-1"
        }, CancellationToken.None);

        _events.Verify(e => e.PublishEventAsync(
            It.IsAny<CustomProductSubmittedIntegrationEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUoWFails_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByTaskIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomProduct?)null);
        _repo.Setup(r => r.Add(It.IsAny<CustomProduct>()));
        _uow.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await _handler.Handle(new SubmitCustomProductCommand
        {
            CustomProduct = CustomProductFixtures.BuildSubmitRequest(),
            OwnerUserId = "user-1"
        }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }
}
