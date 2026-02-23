using System;

namespace Application.Product.DTOs;

public class ProductDto
{
    public required string Id { get; set; }
    public required string Name { get; set; } 
    public required string Description { get; set; }
    public long Price { get; set; }
    public required string PictureUrl { get; set; }
    public required string  Type { get; set; }
    public required string Brand { get; set; }
    public int QuantityInStock { get; set; }
}
