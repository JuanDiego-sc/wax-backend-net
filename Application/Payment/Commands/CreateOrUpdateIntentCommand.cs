using System;
using Application.Basket.DTOs;
using Application.Core;
using MediatR;

namespace Application.Payment.Commands;

public class CreateOrUpdateIntentCommand : IRequest<Result<BasketDto>>
{
    public required string BasketId { get; set; }
}
