using Application.Interfaces.Repositories;
using Application.Product.Extensions;
using Application.Product.Queries;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Product;

public class GetProductsQueryHandlerTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_WithNoFilters_ReturnsAllProducts()
    {
        using var context = CreateInMemoryContext();
        context.Products.AddRange(
            ProductFixtures.CreateProduct(name: "Alpha"),
            ProductFixtures.CreateProduct(name: "Beta"),
            ProductFixtures.CreateProduct(name: "Gamma"));
        await context.SaveChangesAsync();

        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.Products.AsQueryable());

        var handler = new GetProductsQueryHandler(repoMock.Object);
        var query = new GetProductsQuery { ProductParams = new ProductParams { PageNumber = 1, PageSize = 10 } };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Should().HaveCount(3);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_FiltersByName()
    {
        using var context = CreateInMemoryContext();
        context.Products.AddRange(
            ProductFixtures.CreateProduct(name: "Candle Light"),
            ProductFixtures.CreateProduct(name: "Wax Seal"),
            ProductFixtures.CreateProduct(name: "Candle Holder"));
        await context.SaveChangesAsync();

        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.Products.AsQueryable());

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
        using var context = CreateInMemoryContext();
        context.Products.AddRange(
            ProductFixtures.CreateProduct(name: "A", brand: "WaxCo"),
            ProductFixtures.CreateProduct(name: "B", brand: "OtherBrand"),
            ProductFixtures.CreateProduct(name: "C", brand: "WaxCo"));
        await context.SaveChangesAsync();

        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.Products.AsQueryable());

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
        using var context = CreateInMemoryContext();
        for (var i = 1; i <= 5; i++)
            context.Products.Add(ProductFixtures.CreateProduct(name: $"Product {i}"));
        await context.SaveChangesAsync();

        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.Products.AsQueryable());

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
