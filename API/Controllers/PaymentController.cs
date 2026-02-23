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
}
