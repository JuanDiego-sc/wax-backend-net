using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ProductDomain = Domain.Entities.Product;

namespace Application.Product.DTOs;

public record CreateProductDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    [Required]
    [Range(100, double.PositiveInfinity)]
    public long Price { get; set; }
    [Required]
    public IFormFile File { get; set; } = null!;
    public string? PictureUrl { get; set; }
    [Required]
    public string Type { get; set; } = string.Empty;
    [Required]
    public string Brand { get; set; } = string.Empty;
    [Required]
    [Range(0, 200)]
    public int QuantityInStock { get; set; }
    public string? PublicId { get; set; }
    public ProductDomain ToEntity() => new()
    {
        Name = Name,
        Description = Description,
        Price = Price,
        PictureUrl = PictureUrl ?? string.Empty,
        Type = Type,
        Brand = Brand,
        QuantityInStock = QuantityInStock,
        PublicId = PublicId
    };

}
