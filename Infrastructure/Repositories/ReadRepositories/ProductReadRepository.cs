using System.Linq.Expressions;
using Application.Interfaces.Repositories.ReadRepositories;
using Application.Product.DTOs;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.ReadModels;

namespace Infrastructure.Repositories.ReadRepositories;

public class ProductReadRepository(ReadDbContext context) : IProductReadRepository
{
    public async Task<ProductDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await context.Products
            .Where(p => p.Id == id)
            .Select(MapToDto)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public IQueryable<ProductDto> GetQueryable()
    {
        return context.Products.Select(MapToDto);
    }

    private static readonly Expression<Func<ProductReadModel, ProductDto>> MapToDto = p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        Description = p.Description,
        Price = p.Price,
        PictureUrl = p.PictureUrl,
        Type = p.Type,
        Brand = p.Brand,
        QuantityInStock = p.QuantityInStock,
        PublicId = p.PublicId,
    };
}