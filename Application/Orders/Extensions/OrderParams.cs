using Application.Core.Pagination;

namespace Application.Orders.Extensions;

public class OrderParams : PaginationParams
{
    public string? Filter { get; set; }
}
