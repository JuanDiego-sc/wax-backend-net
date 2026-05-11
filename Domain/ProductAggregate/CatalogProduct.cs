using Domain.Enumerators;

namespace Domain.ProductAggregate;

public class CatalogProduct : Product
{
    public required string  Type { get; set; }
    public required string Brand { get; set; }
    public int QuantityInStock { get; set; }
    public string? PublicId { get; set; }

    public override string Kind => ProductTypes.Catalog;
}