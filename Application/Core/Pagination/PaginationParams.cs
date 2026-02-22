using System;

namespace Application.Core.Pagination;

public class PaginationParams
{
    private const int maxPageSize = 30;
    public int PageNumber { get; set; } = 1;
    private int _pageSize = 8;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > maxPageSize ? maxPageSize : value;
    }
}
