namespace Application.Interfaces.DTOs;

public record QuotationResult(long Amount, IReadOnlyDictionary<string, decimal> Breakdown);