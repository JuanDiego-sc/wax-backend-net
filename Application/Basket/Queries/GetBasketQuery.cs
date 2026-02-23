using System;
using Application.Basket.DTOs;
using Application.Core;
using MediatR;

namespace Application.Basket.Queries;

public class GetBasketQuery : IRequest<Result<BasketDto>>
{
    public required string BasketId { get; set; }
}
