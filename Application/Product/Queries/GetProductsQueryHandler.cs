using Application.Core;
using Application.Core.Pagination;
using Application.Interfaces.Repositories;
using Application.Product.DTOs;
using Application.Product.Extensions;
using MediatR;

namespace Application.Product.Queries;

public class GetProductsQueryHandler(IProductRepository productRepository) 
    : IRequestHandler<GetProductsQuery, Result<PagedList<ProductDto>>>
{
    public async Task<Result<PagedList<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var productQuery = productRepository.GetQueryable()
            .Sort(request.ProductParams.OrderBy)
            .Search(request.ProductParams.SearchTerm)
            .Filter(request.ProductParams.Brands, request.ProductParams.Types)
            .Select(x => x.ToDto());

        var products = await PagedList<ProductDto>.ToPagedList(productQuery,
            request.ProductParams.PageNumber, request.ProductParams.PageSize);

        return Result<PagedList<ProductDto>>.Success(products);
    }
}
