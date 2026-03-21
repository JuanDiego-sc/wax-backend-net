using Domain.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations.OrderConfigurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");
        
        builder.HasKey(oi => oi.Id);
        
        builder.OwnsOne(oi => oi.ItemOrdered, pio =>
        {
            pio.Property(p => p.ProductId).HasColumnName("Item_ProductId");
            pio.Property(p => p.Name).HasColumnName("Item_ProductName");
        });
    }
}