namespace Application.Interfaces.Services;

public interface IQuotationRulesCache
{
    Task<decimal> GetAsync(string key, decimal fallback, CancellationToken cancellationToken = default);
    Task InvalidateAsync(CancellationToken cancellationToken = default);
}