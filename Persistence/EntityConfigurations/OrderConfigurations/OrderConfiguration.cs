using Domain.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations.OrderConfigurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.BuyerEmail).IsRequired().HasMaxLength(200);
        builder.Property(o => o.PaymentIntentId).IsRequired().HasMaxLength(200);
        builder.Property(o => o.BillingAddressId).IsRequired().HasMaxLength(200);
        
        
        builder.OwnsOne(o => o.PaymentSummary, ps =>
        {
            ps.Property(p => p.Last4).HasColumnName("Payment_Last4");
            ps.Property(p => p.Brand).HasColumnName("Payment_Brand");
            ps.Property(p => p.ExpMonth).HasColumnName("Payment_ExpMonth");
            ps.Property(p => p.ExpYear).HasColumnName("Payment_ExpYear");
        });
        
        builder.HasMany(o => o.OrderItems)
            .WithOne()
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(o => o.BillingAddress)
            .WithMany()
            .HasForeignKey(o => o.BillingAddressId);
    }
}