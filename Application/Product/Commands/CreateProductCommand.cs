using System;
using Application.Core;
using Application.Product.DTOs;
using MediatR;

namespace Application.Product.Commands;

public class CreateProductCommand : IRequest<Result<CreateProductDto>>
{
    public required CreateProductDto ProductDto{ get; set; }
}
