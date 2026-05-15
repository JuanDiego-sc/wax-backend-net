using Application.Core.Validations;
using Application.IntegrationEvents.ProductEvents;
using Application.Interfaces.Publish;
using Application.Interfaces.Repositories.WriteRepositories;
using Application.Product.Commands;
using Application.Product.Commands.Delete;
using Domain.Enumerators;
using MediatR;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Product;

public class DeleteProductCommandHandlerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IEventPublisher> _eventPublisher = new();
    private readonly Mock<IProductDeletionStrategy> _catalogStrategy = new();
    private readonly Mock<IProductDeletionStrategy> _customStrategy = new();
    private readonly DeleteProductCommandHandler _handler;

    public DeleteProductCommandHandlerTests()
    {
        _catalogStrategy.SetupGet(s => s.Kind).Returns(ProductTypes.Catalog);
        _customStrategy.SetupGet(s => s.Kind).Returns(ProductTypes.Custom);

        _handler = new DeleteProductCommandHandler(
            _productRepo.Object,
            new[] { _catalogStrategy.Object, _customStrategy.Object },
            _unitOfWork.Object,
            _eventPublisher.Object);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsFailure()
    {
        _productRepo
            .Setup(r => r.FindAnyByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((global::Domain.ProductAggregate.Product?)null);

        var result = await _handler.Handle(new DeleteProductCommand { ProductId = "missing" }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Product not found");
        result.Code.Should().Be(404);
    }

    [Fact]
    public async Task Handle_WhenCatalogProduct_DispatchesToCatalogStrategy()
    {
        var product = ProductFixtures.CreateProduct(publicId: "pub123");
        _productRepo
            .Setup(r => r.FindAnyByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _catalogStrategy
            .Setup(s => s.ExecuteAsync(product, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(new DeleteProductCommand { ProductId = product.Id }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _catalogStrategy.Verify(s => s.ExecuteAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _customStrategy.Verify(s => s.ExecuteAsync(It.IsAny<global::Domain.ProductAggregate.Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _eventPublisher.Verify(p => p.PublishEventAsync(
            It.Is<ProductDeletedIntegrationEvent>(e => e.ProductId == product.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCustomProduct_DispatchesToCustomStrategy()
    {
        var custom = CustomProductFixtures.CreateCustomProduct("cp-del");
        _productRepo
            .Setup(r => r.FindAnyByIdAsync("cp-del", It.IsAny<CancellationToken>()))
            .ReturnsAsync(custom);
        _customStrategy
            .Setup(s => s.ExecuteAsync(custom, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(new DeleteProductCommand { ProductId = "cp-del" }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _customStrategy.Verify(s => s.ExecuteAsync(custom, It.IsAny<CancellationToken>()), Times.Once);
        _catalogStrategy.Verify(s => s.ExecuteAsync(It.IsAny<global::Domain.ProductAggregate.Product>(), It.IsAny<CancellationToken>()), Times.Never);
        _eventPublisher.Verify(p => p.PublishEventAsync(
            It.Is<ProductDeletedIntegrationEvent>(e => e.ProductId == "cp-del"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenStrategyFails_ReturnsFailureAndDoesNotPublish()
    {
        var product = ProductFixtures.CreateProduct();
        _productRepo
            .Setup(r => r.FindAnyByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _catalogStrategy
            .Setup(s => s.ExecuteAsync(product, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Failure("boom"));

        var result = await _handler.Handle(new DeleteProductCommand { ProductId = product.Id }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("boom");
        _eventPublisher.Verify(p => p.PublishEventAsync(
            It.IsAny<ProductDeletedIntegrationEvent>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.CompleteAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUnitOfWorkFails_ReturnsFailure()
    {
        var product = ProductFixtures.CreateProduct();
        _productRepo
            .Setup(r => r.FindAnyByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _catalogStrategy
            .Setup(s => s.ExecuteAsync(product, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<Unit>.Success(Unit.Value));
        _unitOfWork
            .Setup(u => u.CompleteAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(new DeleteProductCommand { ProductId = product.Id }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to delete product");
    }
}
