using System;

namespace Application.Core.Pagination;

public class PaginationMetadata
{
    public int TotalCount { get; set; } // Total number of items across all pages
    public int PageSize { get; set; }  // Number of items per page
    public int CurrentPage { get; set; } // Current page number
    public int TotalPages { get; set; } // Total number of pages based on total count and page size
}
