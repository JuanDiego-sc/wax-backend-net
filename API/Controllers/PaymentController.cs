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
        
        return await HandleCommand(new CreateOrUpdateIntentCommand { BasketId = basketId });
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook()
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();
        
        return await HandleCommand(new HandleStripeWebhookCommand { Signature = signature, Payload = payload });
    }
}