using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.ReadModels;

namespace Persistence.EntityConfigurations.ProductConfigurations;

public class CustomProductReadConfiguration : IEntityTypeConfiguration<CustomProductReadModel>
{
    public void Configure(EntityTypeBuilder<CustomProductReadModel> builder)
    {
        builder.ToTable("CustomProductsRead");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Property(p => p.Description).IsRequired();
        builder.Property(p => p.PictureUrl).IsRequired().HasMaxLength(1000);
        builder.Property(p => p.TaskId).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.TaskId).IsUnique();

        builder.Property(p => p.GlbUrl).IsRequired().HasMaxLength(1000);
        builder.Property(p => p.OwnerUserId).IsRequired().HasMaxLength(450);
        builder.Property(p => p.Status).IsRequired().HasMaxLength(40);
        builder.HasIndex(p => new { p.OwnerUserId, p.Status });

        builder.Property(p => p.DesignType).HasMaxLength(100);
        builder.Property(p => p.DesignMaterial).HasMaxLength(100);
        builder.Property(p => p.DesignColor).HasMaxLength(50);
        builder.Property(p => p.DesignShape).HasMaxLength(50);
        builder.Property(p => p.DesignDimensions).HasMaxLength(50);
        builder.Property(p => p.DesignDetails).HasMaxLength(500);
    }
}