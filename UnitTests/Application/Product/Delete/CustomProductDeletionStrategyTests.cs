using Application.Interfaces.Repositories.WriteRepositories;
using Application.Product.Commands.Delete;
using Domain.Enumerators;
using Domain.ProductAggregate;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Product.Delete;

public class CustomProductDeletionStrategyTests
{
    private readonly Mock<ICustomProductRepository> _customRepo = new();
    private readonly CustomProductDeletionStrategy _strategy;

    public CustomProductDeletionStrategyTests()
    {
        _strategy = new CustomProductDeletionStrategy(_customRepo.Object);
    }

    [Fact]
    public void Kind_IsCustom()
    {
        _strategy.Kind.Should().Be(ProductTypes.Custom);
    }

    [Fact]
    public async Task ExecuteAsync_RemovesCustomProduct()
    {
        var product = CustomProductFixtures.CreateCustomProduct("cp-1");

        var result = await _strategy.ExecuteAsync(product, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _customRepo.Verify(r => r.Remove(product), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWrongProductType_ReturnsFailure()
    {
        var catalog = ProductFixtures.CreateProduct();

        var result = await _strategy.ExecuteAsync(catalog, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _customRepo.Verify(r => r.Remove(It.IsAny<CustomProduct>()), Times.Never);
    }
}
