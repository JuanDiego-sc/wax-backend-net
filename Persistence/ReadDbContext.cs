using Microsoft.EntityFrameworkCore;
using Persistence.EntityConfigurations.ProductConfigurations;
using Persistence.ReadModels;

namespace Persistence;

public class ReadDbContext(DbContextOptions<ReadDbContext> options) : DbContext(options)
{
    public DbSet<ProductReadModel> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfiguration(new ProductReadConfiguration());
    }
}