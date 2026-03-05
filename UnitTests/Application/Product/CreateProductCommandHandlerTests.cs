using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Interfaces.DTOs;
using Application.Product.Commands;
using Application.Product.DTOs;
using Moq;
using UnitTests.Helpers;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Product;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IImageService> _imageService = new();
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _handler = new CreateProductCommandHandler(
            _productRepo.Object,
            _unitOfWork.Object,
            _imageService.Object);
    }

    [Fact]
    public async Task Handle_WhenImageUploadFails_ReturnsFailure()
    {
        var dto = new CreateProductDto
        {
            Name = "Product",
            Description = "Desc",
            Price = 1000,
            File = new FormFileMock("image.jpg"),
            Type = "Resin",
            Brand = "Brand",
            QuantityInStock = 5
        };

        _imageService
            .Setup(s => s.UploadImage(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>()))
            .ReturnsAsync((ImageUploadResult?)null);

        var command = new CreateProductCommand { ProductDto = dto };
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

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = new CreateProductCommand { ProductDto = dto };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to create product");
    }

    [Fact]
    public async Task Handle_WithNoFile_SkipsImageUpload_AndReturnsSuccess()
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

        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateProductCommand { ProductDto = dto };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _imageService.Verify(s => s.UploadImage(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithFile_UploadsImageAndAssignsUrlAndPublicId()
    {
        var dto = new CreateProductDto
        {
            Name = "Product",
            Description = "Desc",
            Price = 1000,
            File = new FormFileMock("image.jpg"),
            Type = "Resin",
            Brand = "Brand",
            QuantityInStock = 5
        };

        var uploadResult = new ImageUploadResult { Url = "https://cdn/img.jpg", PublicId = "pub123" };
        _imageService
            .Setup(s => s.UploadImage(It.IsAny<Microsoft.AspNetCore.Http.IFormFile>()))
            .ReturnsAsync(uploadResult);
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateProductCommand { ProductDto = dto };
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PictureUrl.Should().Be("https://cdn/img.jpg");
        result.Value.PublicId.Should().Be("pub123");
    }
}
