using System;
using Application.Core;
using Application.Product.DTOs;
using MediatR;

namespace Application.Product.Queries;

public class GetProductDetailsQuery : IRequest<Result<ProductDto>>
{
    public required string Id { get; set; }
}
