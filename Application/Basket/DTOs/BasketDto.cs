using System;

namespace Application.Basket.DTOs;

public record BasketDto
{
    public required string BasketId { get; init; }
    public List<BasketItemDto>  Items { get; init; } = [];
    public string? ClientSecret { get; init; }  
    public string? PaymentIntentId { get; init; }
}
