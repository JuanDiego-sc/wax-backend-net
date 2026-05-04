using Application.Interfaces;
using Application.Interfaces.DTOs;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Product.Commands;
using Application.Product.DTOs;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Product;

public class UpdateProductAdditionalTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IImageService> _imageService = new();
    private readonly Mock<IEventPublisher> _eventPublisher = new();
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductAdditionalTests()
    {
        _handler = new UpdateProductCommandHandler(
            _productRepo.Object,
            _unitOfWork.Object,
            _imageService.Object,
            _eventPublisher.Object);
    }

    private static UpdateProductDto BuildDto(string productId) => new()
    {
        Id = productId,
        Name = "Updated Name",
        Description = "Updated Desc",
        Price = 2000,
        Type = "Resin",
        Brand = "Brand"
    };

    [Fact]
    public async Task Handle_WhenProductHasExistingPublicId_DeletesOldImageBeforeUpload()
    {
        // The DTO must carry the existing PublicId — ApplyTo sets product.PublicId = dto.PublicId
        // so the handler reads product.PublicId AFTER ApplyTo to decide whether to delete
        var product = ProductFixtures.CreateProduct(publicId: "existing-public-id");
        _productRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var imageResult = new ImageUploadResult { Url = "https://new.img", PublicId = "new-public-id" };
        _imageService
            .Setup(s => s.UploadImage(It.IsAny<ImageUploadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageResult);
        _imageService
            .Setup(s => s.DeleteImage(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("deleted");

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var imageRequest = new ImageUploadRequest(new MemoryStream(), "new.jpg", "image/jpeg");
        // DTO carries the existing PublicId so ApplyTo preserves it on product
        var dto = new UpdateProductDto
        {
            Id = product.Id,
            Name = "Updated Name",
            Description = "Updated Desc",
            Price = 2000,
            Type = "Resin",
            Brand = "Brand",
            PublicId = "existing-public-id"   // preserve old PublicId in DTO
        };
        var command = new UpdateProductCommand { ProductDto = dto, ImageRequest = imageRequest };

        await _handler.Handle(command, CancellationToken.None);

        _imageService.Verify(
            s => s.DeleteImage("existing-public-id", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenProductHasNoExistingPublicId_SkipsDeleteAndUploads()
    {
        var product = ProductFixtures.CreateProduct(publicId: null);
        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var imageResult = new ImageUploadResult { Url = "https://new.img", PublicId = "new-public-id" };
        _imageService
            .Setup(s => s.UploadImage(It.IsAny<ImageUploadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(imageResult);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var imageRequest = new ImageUploadRequest(new MemoryStream(), "new.jpg", "image/jpeg");
        var command = new UpdateProductCommand { ProductDto = BuildDto(product.Id), ImageRequest = imageRequest };

        var result = await _handler.Handle(command, CancellationToken.None);

        _imageService.Verify(
            s => s.DeleteImage(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _imageService.Verify(
            s => s.UploadImage(It.IsAny<ImageUploadRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenImageUploadFails_ReturnsFailure()
    {
        var product = ProductFixtures.CreateProduct(publicId: "pub-id");
        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _imageService
            .Setup(s => s.DeleteImage(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("deleted");

        _imageService
            .Setup(s => s.UploadImage(It.IsAny<ImageUploadRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImageUploadResult?)null);

        var imageRequest = new ImageUploadRequest(new MemoryStream(), "new.jpg", "image/jpeg");
        var command = new UpdateProductCommand { ProductDto = BuildDto(product.Id), ImageRequest = imageRequest };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Image upload failed");
    }

    [Fact]
    public async Task Handle_WhenSaveFails_ReturnsFailure()
    {
        var product = ProductFixtures.CreateProduct();
        _productRepo
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new UpdateProductCommand { ProductDto = BuildDto(product.Id) };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to update product");
    }
}
