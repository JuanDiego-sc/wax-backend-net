using Domain.Entities;
using Domain.OrderAggregate;
using Domain.SupportAssistAggregate;
using MassTransit;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Persistence.EntityConfigurations.BasketConfigurations;
using Persistence.EntityConfigurations.OrderConfigurations;
using Persistence.EntityConfigurations.ProductConfigurations;
using Persistence.EntityConfigurations.SupportTicketConfigurations;

namespace Persistence;

public class WriteDbContext(DbContextOptions<WriteDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Basket> Baskets { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<SupportTicket> SupportTickets { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfiguration(new ProductConfiguration());
        builder.ApplyConfiguration(new OrderConfiguration());
        builder.ApplyConfiguration(new OrderItemConfiguration());
        builder.ApplyConfiguration(new BasketConfiguration());
        builder.ApplyConfiguration(new BasketItemConfiguration());
        builder.ApplyConfiguration(new SupportTicketConfiguration());
        
        //MassTransit outbox tables
        builder.AddInboxStateEntity();
        builder.AddOutboxMessageEntity();
        builder.AddOutboxStateEntity();
    }
    
}
