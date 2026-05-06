using Application.Interfaces.Repositories.ReadRepositories;
using Application.Product.DTOs;
using Application.Product.Queries;
using Moq;
using UnitTests.Helpers;

namespace UnitTests.Application.Product;

public class GetProductDetailsQueryHandlerTests
{
    private static ProductDto CreateDto(string id, string name = "N") => new()
    {
        Id = id,
        Name = name,
        Description = "D",
        Price = 1000,
        PictureUrl = "P",
        Brand = "B",
        Type = "T",
        QuantityInStock = 10,
        PublicId = null
    };

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsFailureWith404()
    {
        var repoMock = new Mock<IProductReadRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(new TestAsyncEnumerable<ProductDto>(new List<ProductDto>()));

        var handler = new GetProductDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(new GetProductDetailsQuery { Id = "missing" }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Product not found");
        result.Code.Should().Be(404);
    }

    [Fact]
    public async Task Handle_WhenProductFound_ReturnsMappedDto()
    {
        var productId = Guid.NewGuid().ToString();
        var productDto = CreateDto(productId, "Test Item");
        
        var data = new TestAsyncEnumerable<ProductDto>(new List<ProductDto> { productDto });

        var repoMock = new Mock<IProductReadRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(data);

        var handler = new GetProductDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(new GetProductDetailsQuery { Id = productId }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Test Item");
    }
}
