using Application.Interfaces.DTOs;
using Domain.ProductAggregate;

namespace Application.Interfaces.Services;

public interface IQuotationService
{
    Task<QuotationResult> QuoteAsync(CustomProductDesign design, CancellationToken cancellationToken = default);
}