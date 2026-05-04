using API.Controllers;
using Application.Core;
using Application.Core.Pagination;
using Application.Core.Validations;
using Application.Interfaces.DTOs;
using Application.Product.Commands;
using Application.Product.DTOs;
using Application.Product.Extensions;
using Application.Product.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UnitTests.Helpers;

namespace UnitTests.API.Controllers;

public class ProductControllerTests
{
    private readonly Mock<IMediator> _mediator;
    private readonly ProductController _controller;

    public ProductControllerTests()
    {
        (_mediator, _controller) = ControllerTestFactory.Create(new ProductController());
    }

    // ── GetProducts ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProducts_DelegatesToMediatorWithCorrectQuery()
    {
        var productParams = new ProductParams { PageNumber = 1, PageSize = 10 };
        var pagedList = new PagedList<ProductDto>(new List<ProductDto>(), 0, 1, 10);

        _mediator
            .Setup(m => m.Send(It.Is<GetProductsQuery>(q => q.ProductParams == productParams), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<ProductDto>>.Success(pagedList));

        await _controller.GetProducts(productParams);

        _mediator.Verify(
            m => m.Send(It.Is<GetProductsQuery>(q => q.ProductParams == productParams), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProducts_ReturnsPaginationHeader_WhenSuccess()
    {
        var productParams = new ProductParams { PageNumber = 1, PageSize = 10 };
        var pagedList = new PagedList<ProductDto>(new List<ProductDto>(), 0, 1, 10);

        _mediator
            .Setup(m => m.Send(It.IsAny<GetProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<ProductDto>>.Success(pagedList));

        var result = await _controller.GetProducts(productParams);

        result.Result.Should().BeOfType<OkObjectResult>();
        _controller.Response.Headers.ContainsKey("Pagination").Should().BeTrue();
    }

    [Fact]
    public async Task GetProducts_ReturnsNotFound_WhenResultIs404()
    {
        var productParams = new ProductParams();

        _mediator
            .Setup(m => m.Send(It.IsAny<GetProductsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<PagedList<ProductDto>>.Failure("Not found", 404));

        var result = await _controller.GetProducts(productParams);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ── GetProductDetails ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetProductDetails_DelegatesToMediatorWithCorrectId()
    {
        const string productId = "prod-123";
        var dto = BuildProductDto(productId);

        _mediator
            .Setup(m => m.Send(It.Is<GetProductDetailsQuery>(q => q.Id == productId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductDto>.Success(dto));

        await _controller.GetProductDetails(productId);

        _mediator.Verify(
            m => m.Send(It.Is<GetProductDetailsQuery>(q => q.Id == productId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetProductDetails_ReturnsOk_WhenSuccess()
    {
        var dto = BuildProductDto("prod-1");

        _mediator
            .Setup(m => m.Send(It.IsAny<GetProductDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductDto>.Success(dto));

        var result = await _controller.GetProductDetails("prod-1");

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(dto);
    }

    [Fact]
    public async Task GetProductDetails_ReturnsNotFound_WhenResultIs404()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<GetProductDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductDto>.Failure("Product not found", 404));

        var result = await _controller.GetProductDetails("missing");

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetProductDetails_ReturnsBadRequest_WhenFailure()
    {
        _mediator
            .Setup(m => m.Send(It.IsAny<GetProductDetailsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductDto>.Failure("Something went wrong"));

        var result = await _controller.GetProductDetails("prod-1");

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── CreateProduct ─────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateProduct_BuildsImageRequestAndSendsCommand()
    {
        var productDto = new CreateProductDto
        {
            Name = "New Product",
            Description = "Desc",
            Price = 1000,
            Type = "Type",
            Brand = "Brand",
            QuantityInStock = 5
        };
        var file = new FormFileMock("test.jpg");
        var returnedDto = new CreateProductDto
        {
            Name = "New Product",
            Description = "Desc",
            Price = 1000,
            Type = "Type",
            Brand = "Brand",
            QuantityInStock = 5
        };

        ImageUploadRequest? capturedRequest = null;
        _mediator
            .Setup(m => m.Send(It.IsAny<CreateProductCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<CreateProductDto>>, CancellationToken>((cmd, _) =>
            {
                capturedRequest = ((CreateProductCommand)cmd).ImageRequest;
            })
            .ReturnsAsync(Result<CreateProductDto>.Success(returnedDto));

        await _controller.CreateProduct(productDto, file);

        _mediator.Verify(
            m => m.Send(It.Is<CreateProductCommand>(c =>
                c.ProductDto == productDto &&
                c.ImageRequest != null), It.IsAny<CancellationToken>()),
            Times.Once);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.FileName.Should().Be("test.jpg");
        capturedRequest.ContentType.Should().Be("image/jpeg");
    }

    // ── UpdateProduct ─────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateProduct_WithFile_BuildsImageRequest()
    {
        var productDto = new UpdateProductDto { Id = "prod-1" };
        var file = new FormFileMock("updated.jpg");

        ImageUploadRequest? capturedRequest = null;
        _mediator
            .Setup(m => m.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<Unit>>, CancellationToken>((cmd, _) =>
            {
                capturedRequest = ((UpdateProductCommand)cmd).ImageRequest;
            })
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));

        await _controller.UpdateProduct(productDto, file);

        capturedRequest.Should().NotBeNull();
        capturedRequest!.FileName.Should().Be("updated.jpg");
        capturedRequest.ContentType.Should().Be("image/jpeg");
    }

    [Fact]
    public async Task UpdateProduct_WithoutFile_SendsNullImageRequest()
    {
        var productDto = new UpdateProductDto { Id = "prod-1" };

        ImageUploadRequest? capturedRequest = new ImageUploadRequest(Stream.Null, "dummy", "image/jpeg");
        _mediator
            .Setup(m => m.Send(It.IsAny<UpdateProductCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<Unit>>, CancellationToken>((cmd, _) =>
            {
                capturedRequest = ((UpdateProductCommand)cmd).ImageRequest;
            })
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));

        await _controller.UpdateProduct(productDto, file: null);

        capturedRequest.Should().BeNull();
    }

    // ── DeleteProduct ─────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteProduct_SendsCommandWithCorrectId()
    {
        const string productId = "prod-delete-me";

        _mediator
            .Setup(m => m.Send(It.IsAny<DeleteProductCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));

        await _controller.DeleteProduct(productId);

        _mediator.Verify(
            m => m.Send(It.Is<DeleteProductCommand>(c => c.ProductId == productId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ProductDto BuildProductDto(string id) => new()
    {
        Id = id,
        Name = "Test Product",
        Description = "A test product",
        Price = 500,
        PictureUrl = "https://example.com/img.jpg",
        Type = "Wax",
        Brand = "TestBrand",
        QuantityInStock = 10
    };
}
