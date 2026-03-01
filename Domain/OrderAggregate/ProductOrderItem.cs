using System;
using Microsoft.EntityFrameworkCore;

namespace Domain.OrderAggregate;

[Owned]
public class ProductOrderItem
{
    public required string ProductId { get; set; }
    public required string Name { get; set; }
}
