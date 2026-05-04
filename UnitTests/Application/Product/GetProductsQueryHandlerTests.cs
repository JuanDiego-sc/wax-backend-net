using Application.Interfaces.Repositories.ReadRepositories;
using Application.Product.DTOs;
using Application.Product.Queries;
using Application.Product.Extensions;
using Moq;
using UnitTests.Helpers;

namespace UnitTests.Application.Product;

public class GetProductsQueryHandlerTests
{
    private static ProductDto CreateDto(string name = "N", string brand = "B", string type = "T") => new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = name,
        Description = "D",
        Price = 1000,
        PictureUrl = "P",
        Brand = brand,
        Type = type,
        QuantityInStock = 10,
        PublicId = null
    };

    [Fact]
    public async Task Handle_WithNoFilters_ReturnsAllProducts()
    {
        var data = new TestAsyncEnumerable<ProductDto>(new List<ProductDto>
        {
            CreateDto(name: "Alpha"),
            CreateDto(name: "Beta"),
            CreateDto(name: "Gamma")
        });

        var repoMock = new Mock<IProductReadRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(data);

        var handler = new GetProductsQueryHandler(repoMock.Object);
        var query = new GetProductsQuery { ProductParams = new ProductParams { PageNumber = 1, PageSize = 10 } };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_FiltersByName()
    {
        var data = new TestAsyncEnumerable<ProductDto>(new List<ProductDto>
        {
            CreateDto(name: "Candle Light"),
            CreateDto(name: "Wax Seal"),
            CreateDto(name: "Candle Holder")
        });

        var repoMock = new Mock<IProductReadRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(data);

        var handler = new GetProductsQueryHandler(repoMock.Object);
        var query = new GetProductsQuery
        {
            ProductParams = new ProductParams { SearchTerm = "candle", PageNumber = 1, PageSize = 10 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithBrandFilter_FiltersByBrand()
    {
        var data = new TestAsyncEnumerable<ProductDto>(new List<ProductDto>
        {
            CreateDto(name: "A", brand: "WaxCo"),
            CreateDto(name: "B", brand: "OtherBrand"),
            CreateDto(name: "C", brand: "WaxCo")
        });

        var repoMock = new Mock<IProductReadRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(data);

        var handler = new GetProductsQueryHandler(repoMock.Object);
        var query = new GetProductsQuery
        {
            ProductParams = new ProductParams { Brands = "waxco", PageNumber = 1, PageSize = 10 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ReturnsPagedResults_WhenExceedsPageSize()
    {
        var list = new List<ProductDto>();
        for (var i = 1; i <= 5; i++)
            list.Add(CreateDto(name: $"Product {i}"));

        var data = new TestAsyncEnumerable<ProductDto>(list);

        var repoMock = new Mock<IProductReadRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(data);

        var handler = new GetProductsQueryHandler(repoMock.Object);
        var query = new GetProductsQuery
        {
            ProductParams = new ProductParams { PageNumber = 1, PageSize = 3 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(3);
    }
}
