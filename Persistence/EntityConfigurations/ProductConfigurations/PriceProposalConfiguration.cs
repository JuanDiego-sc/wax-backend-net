using Domain.ProductAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations.ProductConfigurations;

public class PriceProposalConfiguration : IEntityTypeConfiguration<PriceProposal>
{
    public void Configure(EntityTypeBuilder<PriceProposal> builder)
    {
        builder.ToTable("PriceProposals");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.CustomProductId).IsRequired().HasMaxLength(36);
        builder.Property(p => p.Source).HasConversion<int>();
        builder.Property(p => p.Comment).HasMaxLength(500);

        builder.HasIndex(p => new { p.CustomProductId, p.CreatedAt });

        builder.Ignore(p => p.DomainEvents);
    }
}