using Domain.Entities;

namespace Domain.ProductAggregate;

public class QuotationRule : BaseEntity
{
    public required string Key { get; set; }
    public decimal Value { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}