using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using ProductDomain = Domain.Entities.Product;

namespace Application.Product.DTOs;

public class UpdateProductDto
{
    public required string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public long Price { get; set; }
    public IFormFile? File { get; set; }
    public string? PictureUrl { get; set; }
    public string  Type { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int QuantityInStock { get; set; }
    public string? PublicId { get; set; }
    public ProductDomain ApplyTo(ProductDomain product)
    {
        product.Name = Name;
        product.Description = Description;
        product.Price = Price;
        product.PictureUrl = PictureUrl ?? product.PictureUrl;
        product.Type = Type;
        product.Brand = Brand;
        product.QuantityInStock = QuantityInStock;
        product.PublicId = PublicId;

        return product;
    }
}
