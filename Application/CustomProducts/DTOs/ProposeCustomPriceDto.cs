namespace Application.CustomProducts.DTOs;

public record ProposeCustomPriceDto{
    public long Amount { get; init; }
    public string? Comment { get; init; }
}