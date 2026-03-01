using System;
using Application.Basket.DTOs;
using Application.Payment.Commands;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PaymentController : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<BasketDto>> CreateOrUpdateIntent()
    {
        var basketId = Request.Cookies["basketId"] ?? string.Empty;
        
        var command = new CreateOrUpdateIntentCommand { BasketId = basketId };
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString() ?? string.Empty;
        
        var command = new HandleStripeWebhookCommand
        {
            Payload = payload,
            Signature = signature
        };

        return HandleResult(await Mediator.Send(command));
    }
}