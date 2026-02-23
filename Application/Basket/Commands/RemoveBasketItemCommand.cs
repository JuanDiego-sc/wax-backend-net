using System;
using Application.Core;
using MediatR;

namespace Application.Basket.Commands;

public class RemoveBasketItemCommand : IRequest<Result<Unit>>
{
    public required string ProductId { get; set; }
    public required int Quantity { get; set; }
    public required string BasketId { get; set; }
}
