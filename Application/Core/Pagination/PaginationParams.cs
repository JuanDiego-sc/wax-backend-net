using System;

namespace Application.Core.Pagination;

public class PaginationParams
{
    private const int MaxPageSize = 30;
    public int PageNumber { get; init; } = 1;
    private int _pageSize = 8;
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
