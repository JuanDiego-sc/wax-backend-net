using Application.Interfaces.Repositories.ReadRepositories;
using Application.Interfaces.Repositories.WriteRepositores;
using Application.Product.DTOs;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Repositories.ReadRepositories;

public class ProductReadRepository(ReadDbContext context) : IProductReadRepository
{
    public async Task<ProductDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await context.Products
            .Where(p => p.Id == id)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                PictureUrl = p.PictureUrl,
                Type = p.Type,
                Brand = p.Brand,
                QuantityInStock = p.QuantityInStock,
                PublicId = p.PublicId
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public IQueryable<ProductDto> GetQueryable()
    {
        return context.Products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            PictureUrl = p.PictureUrl,
            Type = p.Type,
            Brand = p.Brand,
            QuantityInStock = p.QuantityInStock,
            PublicId = p.PublicId
        });
    }
}