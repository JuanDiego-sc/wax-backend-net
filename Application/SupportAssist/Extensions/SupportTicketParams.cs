using Application.Core.Pagination;

namespace Application.SupportAssist.Extensions;

public class SupportTicketParams : PaginationParams
{
    public string? OrderBy { get; set; }
    public string? Status { get; set; }
    public string? Category { get; set; }
    public DateTime? CreatedOn  { get; set; }
}