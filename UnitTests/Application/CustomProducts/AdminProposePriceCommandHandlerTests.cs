using Application.CustomProducts.Commands;
using Application.CustomProducts.DTOs;
using Application.IntegrationEvents.CustomProductEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using Domain.ProductAggregate;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.CustomProducts;

public class AdminProposePriceCommandHandlerTests
{
    private readonly Mock<ICustomProductRepository> _repo = new();
    private readonly Mock<IEventPublisher> _events = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly AdminProposePriceCommandHandler _handler;

    public AdminProposePriceCommandHandlerTests()
    {
        _handler = new AdminProposePriceCommandHandler(_repo.Object, _events.Object, _uow.Object);
        _uow.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((CustomProduct?)null);

        var cmd = new AdminProposePriceCommand
        {
            CustomProductId = "x",
            ProposeCustomPrice = new ProposeCustomPriceDto { Amount = 100, Comment = "Test" }
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WhenValid_RegistersProposalAndSucceeds()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1").WithSystemQuotation().WithCustomerCounterOffer();
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var cmd = new AdminProposePriceCommand
        {
            CustomProductId = "cp-1",
            ProposeCustomPrice = new ProposeCustomPriceDto { Amount = 150, Comment = "Admin offer" }
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.Proposals.Should().Contain(p => p.Amount == 150 && p.Source == ProposalSource.Admin);
        
        _events.Verify(e => e.PublishEventAsync(
            It.Is<CustomProductPriceUpdatedIntegrationEvent>(ev =>
                ev.CustomProductId == "cp-1" && ev.Price == 150 && ev.Comment == "Admin offer"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUoWFails_ReturnsFailure()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1").WithSystemQuotation();
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _uow.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var cmd = new AdminProposePriceCommand
        {
            CustomProductId = "cp-1",
            ProposeCustomPrice = new ProposeCustomPriceDto { Amount = 150, Comment = "Admin offer" }
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not updated");
    }
}
