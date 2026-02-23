using System;
using Application.Core;
using Application.Core.Pagination;
using Application.Product.DTOs;
using Application.Product.Extensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Product.Queries;

public class GetProductsQueryHandler(AppDbContext context) : IRequestHandler<GetProductsQuery, Result<List<ProductDto>>>
{
    public async Task<Result<List<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var productQuery = context.Products
            .AsNoTracking()
            .Sort(request.ProductParams.OrderBy)
            .Search(request.ProductParams.SearchTerm)
            .Filter(request.ProductParams.Brands, request.ProductParams.Types)
            .Select(x => x.ToDto())
            .AsQueryable();

        var products = await PagedList<ProductDto>.ToPagedList(productQuery, 
            request.ProductParams.PageNumber, request.ProductParams.PageSize);

        return Result<List<ProductDto>>.Success(products);

    }
}