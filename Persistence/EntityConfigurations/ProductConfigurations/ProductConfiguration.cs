using Domain.Enumerators;
using Domain.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations.ProductConfigurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Product");
        builder.HasKey(x => x.Id);
        
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.PictureUrl).IsRequired().HasMaxLength(1000);

        builder.Ignore(p => p.Kind);
        builder.Ignore(p => p.DomainEvents);

        builder.HasDiscriminator<string>("ProductKind")
            .HasValue<CustomProduct>(ProductTypes.Custom)
            .HasValue<CatalogProduct>(ProductTypes.Catalog);
    }
}