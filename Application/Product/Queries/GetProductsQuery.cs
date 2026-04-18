using System;
using Application.Core;
using Application.Core.Pagination;
using Application.Core.Validations;
using Application.Product.DTOs;
using Application.Product.Extensions;
using MediatR;

namespace Application.Product.Queries;

public class GetProductsQuery : IRequest<Result<PagedList<ProductDto>>>
{
    public required ProductParams ProductParams { get; set; }
}
