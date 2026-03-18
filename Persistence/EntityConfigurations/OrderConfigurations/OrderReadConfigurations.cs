using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.ReadModels;

namespace Persistence.EntityConfigurations.OrderConfigurations;

public class OrderReadConfigurations : IEntityTypeConfiguration<OrderReadModel>
{
    public void Configure(EntityTypeBuilder<OrderReadModel> builder)
    {
        builder.ToTable("Orders");
        builder.HasKey(p => p.Id);
        
        builder.Property(o => o.BuyerEmail).IsRequired().HasMaxLength(200);
        builder.Property(o => o.OrderStatus).IsRequired().HasMaxLength(200);
        
        builder.Property(o => o.BillingName).IsRequired().HasMaxLength(200);
        builder.Property(o => o.BillingLine1).IsRequired().HasMaxLength(200);
        builder.Property(o => o.BillingLine2).IsRequired().HasMaxLength(200);
        builder.Property(o => o.BillingCity).IsRequired().HasMaxLength(200);
        builder.Property(o => o.BillingCountry).IsRequired().HasMaxLength(200);
        builder.Property(o => o.BillingState).IsRequired().HasMaxLength(200);
        builder.Property(o => o.BillingPostalCode).IsRequired().HasMaxLength(200);
        
        builder.Property(o => o.PaymentBrand).IsRequired().HasMaxLength(200);
        
        builder.Property(o => o.OrderItems).IsRequired();
        builder.Property(o => o.PaymentIntentId).IsRequired().HasMaxLength(200);
        
        builder.HasIndex(o => o.BuyerEmail);
        builder.HasIndex(o => o.PaymentIntentId);
        
    }
}