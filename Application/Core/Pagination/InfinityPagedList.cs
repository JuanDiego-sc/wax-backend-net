using System;

namespace Application.Core.Pagination;

public class InfinityPagedList<T, TCursor>
{
    public List<T> Items { get; set; } = [];
    public TCursor? NextCursor { get; set; }
}
