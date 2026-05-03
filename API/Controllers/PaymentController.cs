using Application.Basket.DTOs;
using Application.Basket.Interfaces;
using Application.Payment.Commands;
using Domain.Enumerators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PaymentController(IBasketProvider basketProvider) : BaseApiController
{
    [HttpPost]
    [ProducesResponseType(typeof(BasketDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<ActionResult<BasketDto>> CreateOrUpdateIntent()
    {
        var basketId = basketProvider.GetBasketId() ?? string.Empty;
        
        return await HandleCommand(new CreateOrUpdateIntentCommand { BasketId = basketId });
    }
    
    [HttpPost("webhook")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> StripeWebhook([FromHeader(Name = "Stripe-Signature")] string? signature)
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        
        return await HandleCommand(new HandleStripeWebhookCommand { Signature = signature ?? string.Empty, Payload = payload });
    }
}