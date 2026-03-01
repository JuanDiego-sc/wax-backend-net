using System;

namespace Application.Orders.DTOs;

public record OrderItemDto
{
    public required string ProductId { get; set; }
    public required string Name { get; set; }
    public long Price { get; set; }
    public int Quantity { get; set; }
}
