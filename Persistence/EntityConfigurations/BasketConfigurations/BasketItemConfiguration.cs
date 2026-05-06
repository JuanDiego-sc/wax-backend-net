using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations.BasketConfigurations;

public class BasketItemConfiguration : IEntityTypeConfiguration<BasketItem>
{
    public void Configure(EntityTypeBuilder<BasketItem> builder)
    {
        builder.ToTable("BasketItems");
        
        builder.HasIndex(b => new {b.BasketId, b.ProductId})
            .IsUnique()
            .HasDatabaseName("UniqueBasketItem");
        
        builder.HasOne(b => b.Basket)
            .WithMany(b => b.Items)
            .HasForeignKey(b => b.BasketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}