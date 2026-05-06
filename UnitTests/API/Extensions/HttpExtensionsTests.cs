using System.Text.Json;
using API.Extensions;
using Application.Core.Pagination;
using Microsoft.AspNetCore.Http;

namespace UnitTests.API.Extensions;

public class HttpExtensionsTests
{
    [Fact]
    public void AddPaginationHeader_AddsPaginationHeaderToResponse()
    {
        var context = new DefaultHttpContext();
        var metadata = new PaginationMetadata
        {
            TotalCount = 50,
            PageSize = 10,
            CurrentPage = 2,
            TotalPages = 5
        };

        context.Response.AddPaginationHeader(metadata);

        context.Response.Headers.Should().ContainKey("Pagination");
    }

    [Fact]
    public void AddPaginationHeader_ExposesAccessControlHeaderName()
    {
        var context = new DefaultHttpContext();
        var metadata = new PaginationMetadata
        {
            TotalCount = 20,
            PageSize = 5,
            CurrentPage = 1,
            TotalPages = 4
        };

        context.Response.AddPaginationHeader(metadata);

        context.Response.Headers.Should().ContainKey("Access-Control-Expose-Headers");
        context.Response.Headers["Access-Control-Expose-Headers"].ToString().Should().Contain("Pagination");
    }

    [Fact]
    public void AddPaginationHeader_SerializesMetadataAsCamelCase()
    {
        var context = new DefaultHttpContext();
        var metadata = new PaginationMetadata
        {
            TotalCount = 100,
            PageSize = 20,
            CurrentPage = 3,
            TotalPages = 5
        };

        context.Response.AddPaginationHeader(metadata);

        var headerValue = context.Response.Headers["Pagination"].ToString();
        var doc = JsonDocument.Parse(headerValue);
        var root = doc.RootElement;

        root.TryGetProperty("totalCount", out var totalCount).Should().BeTrue();
        root.TryGetProperty("pageSize", out var pageSize).Should().BeTrue();
        root.TryGetProperty("currentPage", out var currentPage).Should().BeTrue();
        root.TryGetProperty("totalPages", out var totalPages).Should().BeTrue();

        totalCount.GetInt32().Should().Be(100);
        pageSize.GetInt32().Should().Be(20);
        currentPage.GetInt32().Should().Be(3);
        totalPages.GetInt32().Should().Be(5);
    }
}
