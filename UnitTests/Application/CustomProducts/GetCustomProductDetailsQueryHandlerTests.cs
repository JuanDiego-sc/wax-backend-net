using Application.CustomProducts.Queries;
using Application.Interfaces.Repositories.WriteRepositories;
using Domain.ProductAggregate;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.CustomProducts;

public class GetCustomProductDetailsQueryHandlerTests
{
    private readonly Mock<ICustomProductRepository> _repo = new();
    private readonly GetCustomProductDetailsQueryHandler _handler;

    public GetCustomProductDetailsQueryHandlerTests()
    {
        _handler = new GetCustomProductDetailsQueryHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_WhenNotFound_ReturnsFailure()
    {
        _repo.Setup(r => r.GetByIdAsync("x", It.IsAny<CancellationToken>())).ReturnsAsync((CustomProduct?)null);

        var query = new GetCustomProductDetailsQuery { Id = "x", RequesterUserId = "user", RequesterIsAdmin = false };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task Handle_WhenNotOwnerAndNotAdmin_ReturnsAccessDenied()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1", ownerUserId: "owner");
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var query = new GetCustomProductDetailsQuery { Id = "cp-1", RequesterUserId = "not-owner", RequesterIsAdmin = false };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Access denied");
    }

    [Fact]
    public async Task Handle_WhenOwner_ReturnsSuccess()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1", ownerUserId: "owner");
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var query = new GetCustomProductDetailsQuery { Id = "cp-1", RequesterUserId = "owner", RequesterIsAdmin = false };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be("cp-1");
    }

    [Fact]
    public async Task Handle_WhenAdmin_ReturnsSuccess()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1", ownerUserId: "owner");
        _repo.Setup(r => r.GetByIdAsync("cp-1", It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var query = new GetCustomProductDetailsQuery { Id = "cp-1", RequesterUserId = "admin", RequesterIsAdmin = true };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be("cp-1");
    }
}
