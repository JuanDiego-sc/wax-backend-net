using Domain.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations.ProductConfigurations;

public class CustomProductConfiguration :IEntityTypeConfiguration<CustomProduct>
{
    public void Configure(EntityTypeBuilder<CustomProduct> builder)
    {
        builder.Property(p => p.TaskId).IsRequired().HasMaxLength(100);
        builder.HasIndex(p => p.TaskId).IsUnique();

        builder.Property(p => p.GlbUrl).IsRequired().HasMaxLength(1000);
        builder.Property(p => p.RawDescription).IsRequired().HasMaxLength(1000);
        builder.Property(p => p.OwnerUserId).IsRequired().HasMaxLength(450);
        builder.Property(p => p.Status).HasConversion<int>();
        builder.Property(p => p.RejectionReason).HasMaxLength(500);

        builder.HasIndex(p => new { p.OwnerUserId, p.Status });
        
        builder.OwnsOne(p => p.Design, d =>
        {
            d.Property(x => x.Type).HasColumnName("Design_Type").HasMaxLength(100).IsRequired();
            d.Property(x => x.Material).HasColumnName("Design_Material").HasMaxLength(100).IsRequired();
            d.Property(x => x.Color).HasColumnName("Design_Color").HasMaxLength(50).IsRequired();
            d.Property(x => x.Shape).HasColumnName("Design_Shape").HasMaxLength(50).IsRequired();
            d.Property(x => x.Dimensions).HasColumnName("Design_Dimensions").HasMaxLength(50).IsRequired();
            d.Property(x => x.Details).HasColumnName("Design_Details").HasMaxLength(500);
        });

        builder.HasMany(p => p.Proposals)
            .WithOne()
            .HasForeignKey(p => p.CustomProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}