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
        
        //todo: in the write approach this config must be an objetc 
        builder.OwnsOne(o => o.BillingAddress, ba =>
        {
            ba.Property(a => a.Name).HasColumnName("Billing_Address");
            ba.Property(a => a.Line1).HasColumnName("Billing_Line1");
            ba.Property(a => a.Line2).HasColumnName("Billing_Line2");
            ba.Property(a => a.City).HasColumnName("Billing_City");
            ba.Property(a => a.State).HasColumnName("Billing_State");
            ba.Property(a => a.PostalCode).HasColumnName("Billing_PostalCode");
            ba.Property(a => a.Country).HasColumnName("Billing_Country");
        });
        
        //todo: in the write approach this config must be an objetc 
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
    }
}