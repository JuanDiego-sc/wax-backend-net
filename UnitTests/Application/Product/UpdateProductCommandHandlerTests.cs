using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.DTOs;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositores;
using Application.Product.Commands;
using Application.Product.DTOs;
using MediatR;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Product;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IImageService> _imageService = new();
    private readonly Mock<IEventPublisher> _eventPublisher = new();
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _handler = new UpdateProductCommandHandler(
            _productRepo.Object,
            _unitOfWork.Object,
            _imageService.Object,
            _eventPublisher.Object
            );
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsFailure()
    {
        _productRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((global::Domain.Entities.Product?)null);

        var dto = new UpdateProductDto { Id = "missing", Name = "X", Description = "X", Price = 100, Type = "T", Brand = "B" };
        var result = await _handler.Handle(new UpdateProductCommand { ProductDto = dto }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Product not found");
    }

    [Fact]
    public async Task Handle_WhenImageUploadFails_ReturnsFailure()
    {
        var product = ProductFixtures.CreateProduct(publicId: "old-pub");
        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var imageRequest = new ImageUploadRequest(new MemoryStream(), "new.jpg", "image/jpeg");

        _imageService
            .Setup(s => s.UploadImage(imageRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImageUploadResult?)null);

        var dto = new UpdateProductDto
        {
            Id = product.Id,
            Name = "New",
            Description = "New Desc",
            Price = 2000,
            Type = "Resin",
            Brand = "Brand"
        };

        var result = await _handler.Handle(new UpdateProductCommand { ProductDto = dto, ImageRequest = imageRequest }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Image upload failed");
    }

    [Fact]
    public async Task Handle_WhenValid_UpdatesProductAndReturnsSuccess()
    {
        var product = ProductFixtures.CreateProduct();
        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var dto = new UpdateProductDto
        {
            Id = product.Id,
            Name = "Updated",
            Description = "Updated Desc",
            Price = 3000,
            Type = "Resin",
            Brand = "Brand"
        };

        var result = await _handler.Handle(new UpdateProductCommand { ProductDto = dto }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);
    }
}
