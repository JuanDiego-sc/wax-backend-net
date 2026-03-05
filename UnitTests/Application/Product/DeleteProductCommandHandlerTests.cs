using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Product.Commands;
using MediatR;
using Moq;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Product;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IImageService> _imageService = new();
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _handler = new DeleteProductCommandHandler(
            _productRepo.Object,
            _unitOfWork.Object,
            _imageService.Object);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsFailure()
    {
        _productRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((global::Domain.Entities.Product?)null);

        var result = await _handler.Handle(new DeleteProductCommand { ProductId = "missing" }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Product not found");
    }

    [Fact]
    public async Task Handle_WhenProductHasPublicId_DeletesImage()
    {
        var product = ProductFixtures.CreateProduct(publicId: "pub123");
        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _imageService
            .Setup(s => s.DeleteImage("pub123"))
            .ReturnsAsync("deleted");
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(new DeleteProductCommand { ProductId = product.Id }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _imageService.Verify(s => s.DeleteImage("pub123"), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProductHasNoPublicId_SkipsImageDeletion()
    {
        var product = ProductFixtures.CreateProduct(publicId: null);
        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(new DeleteProductCommand { ProductId = product.Id }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _imageService.Verify(s => s.DeleteImage(It.IsAny<string>()), Times.Never);
    }
}
