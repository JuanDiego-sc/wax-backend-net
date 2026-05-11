using Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

namespace Infrastructure.Quotation;

public class QuotationRulesCache(IMemoryCache cache, IServiceProvider serviceProvider) : IQuotationRulesCache
{
    private const string CacheKey = "QuotationRules";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(30);

    public async Task<decimal> GetAsync(string key, decimal fallback, CancellationToken cancellationToken = default)
    {
        Dictionary<string, decimal>? rules;
        try
        {
            rules = await cache.GetOrCreateAsync(CacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = Ttl;
                using var scope = serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<WriteDbContext>();

                return await db.QuotationRules.AsNoTracking()
                    .Where(r => r.IsActive)
                    .ToDictionaryAsync(r => r.Key, r => r.Value, CancellationToken.None);
            });
        }
        catch
        {
            cache.Remove(CacheKey);
            throw;
        }

        cancellationToken.ThrowIfCancellationRequested();
        return rules != null && rules.TryGetValue(key, out var value) ? value : fallback;
    }

    public Task InvalidateAsync(CancellationToken cancellationToken = default)
    {
        cache.Remove(CacheKey);
        return Task.CompletedTask;
    }
}