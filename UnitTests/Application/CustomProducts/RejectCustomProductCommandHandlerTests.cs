using Application.CustomProducts.Commands;
using Application.IntegrationEvents.CustomProductEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using Domain.ProductAggregate;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.CustomProducts;

public class RejectCustomProductCommandHandlerTests
{
    private readonly Mock<ICustomProductRepository> _repo = new();
    private readonly Mock<IEventPublisher> _events = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly RejectCustomProductCommandHandler _handler;

    public RejectCustomProductCommandHandlerTests()
    {
        _handler = new RejectCustomProductCommandHandler(_repo.Object, _events.Object, _uow.Object);
        _uow.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((CustomProduct?)null);

        var cmd = new RejectCustomProductCommand
        {
            CustomProductId = "x",
            Reason = "No reason"
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WhenValid_RejectsAndSucceeds()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1").WithSystemQuotation();
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var cmd = new RejectCustomProductCommand
        {
            CustomProductId = "cp-1",
            Reason = "Cannot be manufactured"
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(CustomProductStatus.Rejected);
        
        _events.Verify(e => e.PublishEventAsync(
            It.Is<CustomProductRejectedIntegrationEvent>(ev =>
                ev.CustomProductId == "cp-1" && ev.Reason == "Cannot be manufactured"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUoWFails_ReturnsFailure()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1").WithSystemQuotation();
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _uow.Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var cmd = new RejectCustomProductCommand
        {
            CustomProductId = "cp-1",
            Reason = "Cannot be manufactured"
        };

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("error while rejecting");
    }
}
