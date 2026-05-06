using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Persistence.EntityConfigurations.UserConfigurations;

namespace UnitTests.Persistence.EntityConfigurations;

public class UserConfigurationTests
{
    private class TestDbContext : IdentityDbContext<User>
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            new UserConfiguration().Configure(builder.Entity<User>());
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
    public void Configure_SetsFirstNameMaxLength()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(User));

        entityType!.FindProperty(nameof(User.FirstName))!.GetMaxLength().Should().Be(50);
    }

    [Fact]
    public void Configure_SetsLastNameMaxLength()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(User));

        entityType!.FindProperty(nameof(User.LastName))!.GetMaxLength().Should().Be(50);
    }

    [Fact]
    public void Configure_SetsPhoneMaxLength()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(User));

        entityType!.FindProperty(nameof(User.Phone))!.GetMaxLength().Should().Be(30);
    }

    [Fact]
    public void Configure_SetsIdentificationNumberMaxLength()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(User));

        entityType!.FindProperty(nameof(User.IdentificationNumber))!.GetMaxLength().Should().Be(20);
    }

    [Fact]
    public void Configure_SetsIdentificationTypeMaxLength()
    {
        using var context = CreateContext();
        var entityType = context.Model.FindEntityType(typeof(User));

        entityType!.FindProperty(nameof(User.IdentificationType))!.GetMaxLength().Should().Be(20);
    }
}
