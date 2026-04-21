using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations.UserConfigurations;

public class BillingAddressConfiguration : IEntityTypeConfiguration<BillingAddress>
{
    public void Configure(EntityTypeBuilder<BillingAddress> builder)
    {
        
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Name).IsRequired().HasMaxLength(50);
        builder.Property(b => b.Line1).IsRequired().HasMaxLength(500);
        builder.Property(b => b.Line2).IsRequired().HasMaxLength(500);
        builder.Property(b => b.City).IsRequired().HasMaxLength(20);
        builder.Property(b => b.State).IsRequired().HasMaxLength(20);
        builder.Property(b => b.PostalCode).IsRequired().HasMaxLength(20);
        builder.Property(b => b.Country).IsRequired().HasMaxLength(20);
        
    }
}