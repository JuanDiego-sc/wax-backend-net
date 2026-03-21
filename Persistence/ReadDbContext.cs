using Microsoft.EntityFrameworkCore;
using Persistence.EntityConfigurations.OrderConfigurations;
using Persistence.EntityConfigurations.ProductConfigurations;
using Persistence.EntityConfigurations.SupportTicketConfigurations;
using Persistence.ReadModels;

namespace Persistence;

public class ReadDbContext(DbContextOptions<ReadDbContext> options) : DbContext(options)
{
    public DbSet<ProductReadModel> Products { get; set; }
    public DbSet<OrderReadModel> Orders { get; set; }
    public DbSet<SupportTicketReadModel> SupportTickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new ProductReadConfiguration());
        modelBuilder.ApplyConfiguration(new OrderReadConfigurations());
        modelBuilder.ApplyConfiguration(new SupportTicketReadConfiguration());
    }
}