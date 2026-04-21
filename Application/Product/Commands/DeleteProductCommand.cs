using System;
using Application.Core;
using Application.Core.Validations;
using MediatR;

namespace Application.Product.Commands;

public class DeleteProductCommand : IRequest<Result<Unit>>
{
    public required string ProductId { get; set; }
}
