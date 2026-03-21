using System;
using Application.Core.Pagination;

namespace Application.Orders.Extensions;

public class OrderParams : InfinityPaginationParams<DateTime?>
{
    public string? Filter { get; set; }
    public DateTime? StartDate { get; set; } = DateTime.UtcNow;
}
