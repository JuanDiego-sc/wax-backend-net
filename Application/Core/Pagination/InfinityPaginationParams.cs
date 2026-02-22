using System;

namespace Application.Core.Pagination;

public class InfinityPaginationParams<TCursor>
{   
    private const int MaxPageSize = 30;
    public TCursor? Cursor { get; set; }
    private int _pageSize = 3;
    public int PageSize
    {        
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
