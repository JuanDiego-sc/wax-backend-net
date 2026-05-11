using Domain.Entities;

namespace Domain.ProductAggregate;

public abstract class Product : BaseEntity
{
    public required string Name { get; set; } 
    public required string Description { get; set; }
    public long Price { get; set; }
    public required string PictureUrl { get; set; }
    
    public abstract string Kind { get; }
}