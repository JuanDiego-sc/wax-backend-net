using Application.Core;
using Application.Core.Pagination;

namespace UnitTests.Application.Core;

public class PagedListTests
{
    [Fact]
    public void Constructor_SetsMetadataCorrectly()
    {
        var items = Enumerable.Range(1, 5).Select(i => i).ToList();

        var pagedList = new PagedList<int>(items, count: 25, pageNumber: 2, pageSize: 5);

        pagedList.Metadata.TotalCount.Should().Be(25);
        pagedList.Metadata.PageSize.Should().Be(5);
        pagedList.Metadata.CurrentPage.Should().Be(2);
        pagedList.Metadata.TotalPages.Should().Be(5);
    }

    [Fact]
    public void Constructor_AddsItemsToList()
    {
        var items = new List<string> { "a", "b", "c" };

        var pagedList = new PagedList<string>(items, count: 3, pageNumber: 1, pageSize: 10);

        pagedList.Should().HaveCount(3);
        pagedList.Should().ContainInOrder("a", "b", "c");
    }

    [Fact]
    public void TotalPages_RoundsUpForPartialPage()
    {
        var items = Enumerable.Range(1, 3).ToList();

        var pagedList = new PagedList<int>(items, count: 11, pageNumber: 1, pageSize: 5);

        pagedList.Metadata.TotalPages.Should().Be(3);
    }

    [Fact]
    public void PaginationParams_PageSizeCappedAtMax()
    {
        var paginationParams = new PaginationParams { PageSize = 999 };

        paginationParams.PageSize.Should().Be(30);
    }
}
