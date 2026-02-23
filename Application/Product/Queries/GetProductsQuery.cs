using System;
using Application.Core;
using Application.Product.DTOs;
using Application.Product.Extensions;
using MediatR;

namespace Application.Product.Queries;

public class GetProductsQuery : IRequest<Result<List<ProductDto>>>
{
    public required ProductParams ProductParams { get; set; }
}
