using System;
using Application.Core;
using Application.Core.Validations;
using Application.Interfaces.DTOs;
using Application.Product.DTOs;
using MediatR;

namespace Application.Product.Commands;

public class UpdateProductCommand : IRequest<Result<Unit>>
{
    public required UpdateProductDto ProductDto { get; set; }
    public ImageUploadRequest? ImageRequest { get; set; }
}
