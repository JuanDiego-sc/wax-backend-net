using Application.Core.Pagination;

namespace UnitTests.Application.Core;

public class InfinityPaginationTests
{
    [Fact]
    public void PageSize_DefaultsTo3()
    {
        var paginationParams = new InfinityPaginationParams<string>();

        paginationParams.PageSize.Should().Be(3);
    }

    [Fact]
    public void PageSize_WhenSetAbove30_ClampsTo30()
    {
        var paginationParams = new InfinityPaginationParams<string>
        {
            PageSize = 50
        };

        paginationParams.PageSize.Should().Be(30);
    }

    [Fact]
    public void PageSize_WhenSetTo10_Keeps10()
    {
        var paginationParams = new InfinityPaginationParams<string>
        {
            PageSize = 10
        };

        paginationParams.PageSize.Should().Be(10);
    }

    [Fact]
    public void PageSize_WhenSetToExactly30_Keeps30()
    {
        var paginationParams = new InfinityPaginationParams<string>
        {
            PageSize = 30
        };

        paginationParams.PageSize.Should().Be(30);
    }

    [Fact]
    public void Cursor_CanBeSetAndRetrieved()
    {
        var paginationParams = new InfinityPaginationParams<string>
        {
            Cursor = "abc-cursor"
        };

        paginationParams.Cursor.Should().Be("abc-cursor");
    }

    [Fact]
    public void Cursor_DefaultsToNull()
    {
        var paginationParams = new InfinityPaginationParams<string>();

        paginationParams.Cursor.Should().BeNull();
    }
}

public class InfinityPagedListTests
{
    [Fact]
    public void Items_CanBeSetAndRetrieved()
    {
        var items = new List<string> { "item1", "item2" };
        var pagedList = new InfinityPagedList<string, string>
        {
            Items = items,
            NextCursor = "next"
        };

        pagedList.Items.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void NextCursor_CanBeSetAndRetrieved()
    {
        var pagedList = new InfinityPagedList<int, string>
        {
            Items = [1, 2, 3],
            NextCursor = "cursor-value"
        };

        pagedList.NextCursor.Should().Be("cursor-value");
    }

    [Fact]
    public void Items_DefaultsToEmptyList()
    {
        var pagedList = new InfinityPagedList<string, int>();

        pagedList.Items.Should().NotBeNull();
        pagedList.Items.Should().BeEmpty();
    }

    [Fact]
    public void NextCursor_DefaultsToNull_WhenNotSet()
    {
        var pagedList = new InfinityPagedList<string, string>();

        pagedList.NextCursor.Should().BeNull();
    }
}
