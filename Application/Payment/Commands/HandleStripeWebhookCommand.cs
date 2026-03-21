using System;
using Application.Core;
using MediatR;

namespace Application.Payment.Commands;

public class HandleStripeWebhookCommand : IRequest<Result<Unit>>
{
    public required string Payload { get; init; }
    public required string Signature { get; init; }
}
