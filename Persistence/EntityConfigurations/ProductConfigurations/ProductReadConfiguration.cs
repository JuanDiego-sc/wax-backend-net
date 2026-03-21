using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.ReadModels;

namespace Persistence.EntityConfigurations.ProductConfigurations;

public class ProductReadConfiguration: IEntityTypeConfiguration<ProductReadModel>
{
    public void Configure(EntityTypeBuilder<ProductReadModel> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).IsRequired().HasMaxLength(500);
        builder.Property(p => p.PictureUrl).IsRequired().HasMaxLength(500);
        builder.Property(p => p.Type).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Brand).IsRequired().HasMaxLength(100);
        builder.Property(p => p.PublicId).HasMaxLength(100);

        builder.HasIndex(p => p.Type);
        builder.HasIndex(p => p.Brand);
        builder.HasIndex(p => p.Name);
    }
}