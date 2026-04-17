using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations.UserConfigurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.BillingAddressId).HasMaxLength(20);
        builder.Property(u => u.FirstName).HasMaxLength(50);
        builder.Property(u => u.LastName).HasMaxLength(50);
        builder.Property(u => u.Phone).HasMaxLength(30);
        builder.Property(u => u.IdentificationType).HasMaxLength(20);
        builder.Property(u => u.IdentificationNumber).HasMaxLength(20);
    }
}