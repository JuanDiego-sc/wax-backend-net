using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Persistence.ReadModels;

namespace Persistence.EntityConfigurations.SupportTicketConfigurations;

public class SupportTicketReadConfiguration : IEntityTypeConfiguration<SupportTicketReadModel>
{
    public void Configure(EntityTypeBuilder<SupportTicketReadModel> builder)
    {
        builder.ToTable("SupportTickets");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).IsRequired().HasMaxLength(450);
        builder.Property(t => t.UserEmail).IsRequired().HasMaxLength(256);
        builder.Property(t => t.UserFullName).IsRequired().HasMaxLength(200);
        builder.Property(t => t.OrderId).IsRequired().HasMaxLength(450);
        builder.Property(t => t.Category).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Status).IsRequired().HasMaxLength(50);
        builder.Property(t => t.Subject).IsRequired().HasMaxLength(500);
        builder.Property(t => t.Description).IsRequired().HasMaxLength(500);

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.OrderId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.Category);
    }
}
