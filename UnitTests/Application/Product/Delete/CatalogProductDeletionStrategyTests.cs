using Application.Interfaces;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Product.Commands.Delete;
using Domain.Enumerators;
using Domain.ProductAggregate;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Product.Delete;

public class CatalogProductDeletionStrategyTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IImageService> _imageService = new();
    private readonly CatalogProductDeletionStrategy _strategy;

    public CatalogProductDeletionStrategyTests()
    {
        _strategy = new CatalogProductDeletionStrategy(_productRepo.Object, _imageService.Object);
    }

    [Fact]
    public void Kind_IsCatalog()
    {
        _strategy.Kind.Should().Be(ProductTypes.Catalog);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPublicIdPresent_DeletesImageAndRemoves()
    {
        var product = ProductFixtures.CreateProduct(publicId: "pub123");
        _imageService.Setup(s => s.DeleteImage("pub123", It.IsAny<CancellationToken>())).ReturnsAsync("deleted");

        var result = await _strategy.ExecuteAsync(product, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _imageService.Verify(s => s.DeleteImage("pub123", It.IsAny<CancellationToken>()), Times.Once);
        _productRepo.Verify(r => r.Remove(product), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoPublicId_SkipsImageDeletion()
    {
        var product = ProductFixtures.CreateProduct(publicId: null);

        var result = await _strategy.ExecuteAsync(product, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _imageService.Verify(s => s.DeleteImage(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _productRepo.Verify(r => r.Remove(product), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWrongProductType_ReturnsFailure()
    {
        var custom = CustomProductFixtures.CreateCustomProduct("cp-1");

        var result = await _strategy.ExecuteAsync(custom, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        _productRepo.Verify(r => r.Remove(It.IsAny<CatalogProduct>()), Times.Never);
    }
}
