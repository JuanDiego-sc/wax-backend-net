using Application.Interfaces.Repositories;
using Application.Product.Queries;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.Product;

public class GetProductDetailsQueryHandlerTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ReturnsFailureWith404()
    {
        using var context = CreateInMemoryContext();

        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.Products.AsQueryable());

        var handler = new GetProductDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(new GetProductDetailsQuery { Id = "missing" }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Product not found");
        result.Code.Should().Be(404);
    }

    [Fact]
    public async Task Handle_WhenProductFound_ReturnsMappedDto()
    {
        using var context = CreateInMemoryContext();
        var product = ProductFixtures.CreateProduct(name: "Test Item");
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.Products.AsQueryable());

        var handler = new GetProductDetailsQueryHandler(repoMock.Object);
        var result = await handler.Handle(new GetProductDetailsQuery { Id = product.Id }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Name.Should().Be("Test Item");
    }
}
