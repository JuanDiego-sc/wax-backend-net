using Application.Interfaces.Services;
using Domain.ProductAggregate;
using Infrastructure.Quotation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

namespace UnitTests.Infrastructure;

public class QuotationRulesCacheTests
{
    private static IServiceProvider BuildServiceProvider(WriteDbContext ctx)
    {
        var services = new ServiceCollection();
        services.AddSingleton(ctx);
        services.AddSingleton<WriteDbContext>(_ => ctx);
        return services.BuildServiceProvider();
    }

    private static WriteDbContext CreateWriteContext(string dbName)
    {
        var opts = new DbContextOptionsBuilder<WriteDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new WriteDbContext(opts);
    }

    [Fact]
    public async Task GetAsync_WhenRuleExists_ReturnsCachedValue()
    {
        using var ctx = CreateWriteContext(Guid.NewGuid().ToString());
        ctx.QuotationRules.Add(new QuotationRule { Key = "BASE_PRICE", Value = 3000, IsActive = true });
        await ctx.SaveChangesAsync();

        var memCache = new MemoryCache(new MemoryCacheOptions());
        var sp = BuildServiceProvider(ctx);
        var cache = new QuotationRulesCache(memCache, sp);

        var result = await cache.GetAsync("BASE_PRICE", fallback: 0);

        result.Should().Be(3000);
    }

    [Fact]
    public async Task GetAsync_WhenKeyMissing_ReturnsFallback()
    {
        using var ctx = CreateWriteContext(Guid.NewGuid().ToString());
        var memCache = new MemoryCache(new MemoryCacheOptions());
        var sp = BuildServiceProvider(ctx);
        var cache = new QuotationRulesCache(memCache, sp);

        var result = await cache.GetAsync("MISSING_KEY", fallback: 99m);

        result.Should().Be(99m);
    }

    [Fact]
    public async Task GetAsync_InactiveRule_IsExcluded()
    {
        using var ctx = CreateWriteContext(Guid.NewGuid().ToString());
        ctx.QuotationRules.Add(new QuotationRule { Key = "INACTIVE", Value = 9999, IsActive = false });
        await ctx.SaveChangesAsync();

        var memCache = new MemoryCache(new MemoryCacheOptions());
        var sp = BuildServiceProvider(ctx);
        var cache = new QuotationRulesCache(memCache, sp);

        var result = await cache.GetAsync("INACTIVE", fallback: 0m);

        result.Should().Be(0m);
    }

    [Fact]
    public async Task InvalidateAsync_ClearsCache_SoNextCallRefreshesFromDb()
    {
        var dbName = Guid.NewGuid().ToString();
        using var ctx = CreateWriteContext(dbName);
        ctx.QuotationRules.Add(new QuotationRule { Key = "PRICE", Value = 1000, IsActive = true });
        await ctx.SaveChangesAsync();

        var memCache = new MemoryCache(new MemoryCacheOptions());
        var sp = BuildServiceProvider(ctx);
        var cache = new QuotationRulesCache(memCache, sp);

        var before = await cache.GetAsync("PRICE", 0m);
        before.Should().Be(1000m);

        ctx.QuotationRules.First().Value = 2000;
        await ctx.SaveChangesAsync();

        await cache.InvalidateAsync();

        var after = await cache.GetAsync("PRICE", 0m);
        after.Should().Be(2000m);
    }
}
