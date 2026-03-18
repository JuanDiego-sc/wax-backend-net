using Application.Interfaces;
using Application.Interfaces.DTOs;
using Application.Interfaces.Repositories.WriteRepositores;
using Application.Interfaces.Publish;
using Application.Product.Commands;
using Application.Product.DTOs;
using Application.IntegrationEvents.ProductEvents;


namespace UnitTests.Application.Product;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IImageService> _imageService = new();
    private readonly Mock<IEventPublisher> _eventPublisher = new();
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _handler = new CreateProductCommandHandler(
            _productRepo.Object,
            _unitOfWork.Object,
            _imageService.Object,
            _eventPublisher.Object);
    }

    [Fact]
    public async Task Handle_WhenImageUploadFails_ReturnsFailure()
    {
        var dto = new CreateProductDto
        {
            Name = "Product",
            Description = "Desc",
            Price = 1000,
            Type = "Resin",
            Brand = "Brand",
            QuantityInStock = 5
        };

        var imageRequest = new ImageUploadRequest(new MemoryStream(), "test.jpg", "image/jpeg");

        _imageService
            .Setup(s => s.UploadImage(imageRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ImageUploadResult?)null);

        var command = new CreateProductCommand { ProductDto = dto, ImageRequest = imageRequest };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Image upload failed");
    }

    [Fact]
    public async Task Handle_WhenUnitOfWorkFails_ReturnsFailure()
    {
        var dto = new CreateProductDto
        {
            Name = "Product",
            Description = "Desc",
            Price = 1000,
            Type = "Resin",
            Brand = "Brand",
            QuantityInStock = 5
        };

        var imageRequest = new ImageUploadRequest(new MemoryStream(), "test.jpg", "image/jpeg");

        _imageService
            .Setup(s => s.UploadImage(imageRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ImageUploadResult { Url = "url", PublicId = "pid" });

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateProductCommand { ProductDto = dto, ImageRequest = imageRequest };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to update product");
    }

    [Fact]
    public async Task Handle_WithFile_UploadsImageAndAssignsUrlAndPublicId()
    {
        var dto = new CreateProductDto
        {
            Name = "Product",
            Description = "Desc",
            Price = 1000,
            Type = "Resin",
            Brand = "Brand",
            QuantityInStock = 5
        };

        var imageRequest = new ImageUploadRequest(new MemoryStream(), "image.jpg", "image/jpeg");
        var uploadResult = new ImageUploadResult { Url = "https://cdn/img.jpg", PublicId = "pub123" };

        _imageService
            .Setup(s => s.UploadImage(imageRequest, It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadResult);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateProductCommand { ProductDto = dto, ImageRequest = imageRequest };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        _eventPublisher.Verify(e => e.PublishEventAsync(
            It.Is<ProductCreatedIntegrationEvent>(ev =>
                ev.Name == dto.Name &&
                ev.PictureUrl == "https://cdn/img.jpg" &&
                ev.PublicId == "pub123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
