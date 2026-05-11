using Domain.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations.ProductConfigurations;

public class CatalogProductConfiguration : IEntityTypeConfiguration<CatalogProduct>
{
    public void Configure(EntityTypeBuilder<CatalogProduct> builder)
    {
        builder.Property(cp => cp.Type).HasMaxLength(50).IsRequired();
        builder.Property(cp => cp.Brand).HasMaxLength(50).IsRequired();
        builder.Property(cp => cp.PublicId).HasMaxLength(50);
    }
}