using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.EntityConfigurations.UserConfigurations;

namespace UnitTests.Persistence.EntityConfigurations;

public class BillingAddressConfigurationTests
{
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }
        public DbSet<BillingAddress> BillingAddresses { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<BillingAddress>().Ignore(b => b.DomainEvents);
            new BillingAddressConfiguration().Configure(builder.Entity<BillingAddress>());
        }
    }

    private static TestDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }

    [Fact]
    public void Configure_SetsNameMaxLength()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(BillingAddress));

        entityType!.FindProperty(nameof(BillingAddress.Name))!.GetMaxLength().Should().Be(50);
    }

    [Fact]
    public void Configure_SetsLine1MaxLength()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(BillingAddress));

        entityType!.FindProperty(nameof(BillingAddress.Line1))!.GetMaxLength().Should().Be(500);
    }

    [Fact]
    public void Configure_SetsCityMaxLength()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(BillingAddress));

        entityType!.FindProperty(nameof(BillingAddress.City))!.GetMaxLength().Should().Be(20);
    }

    [Fact]
    public void Configure_SetsCountryMaxLength()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(BillingAddress));

        entityType!.FindProperty(nameof(BillingAddress.Country))!.GetMaxLength().Should().Be(20);
    }

    [Fact]
    public void Configure_SetsPostalCodeMaxLength()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(BillingAddress));

        entityType!.FindProperty(nameof(BillingAddress.PostalCode))!.GetMaxLength().Should().Be(20);
    }

    [Fact]
    public void Configure_SetsPrimaryKey()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(BillingAddress));

        var key = entityType!.FindPrimaryKey();
        key.Should().NotBeNull();
        key!.Properties.Should().ContainSingle(p => p.Name == nameof(BillingAddress.Id));
    }
}
