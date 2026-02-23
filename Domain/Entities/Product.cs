using System;

namespace Domain.Entities;

public class Product
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Name { get; set; } 
    public required string Description { get; set; }
    public long Price { get; set; }
    public required string PictureUrl { get; set; }
    public required string  Type { get; set; }
    public required string Brand { get; set; }
    public int QuantityInStock { get; set; }
}