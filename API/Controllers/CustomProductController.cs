using Application.Basket.Interfaces;
using Application.CustomProducts.Commands;
using Application.CustomProducts.DTOs;
using Application.CustomProducts.Queries;
using Application.Interfaces.Services;
using Domain.Enumerators;
using Domain.ProductAggregate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class CustomProductController(IUserAccessor userAccessor, IBasketProvider basketProvider) : BaseApiController
{
    [HttpPost]
    [ProducesResponseType(typeof(CustomProductDto), 200)]
    public Task<ActionResult> Submit([FromBody] SubmitCustomProductRequest dto)
    {
        return HandleCommand(new SubmitCustomProductCommand
        {
            CustomProduct = dto,
            OwnerUserId = userAccessor.GetUserId(),
            BasketId = basketProvider.GetBasketId()
        });
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(List<CustomProductDto>), 200)]
    public Task<ActionResult> Mine()
        => HandleQuery(new GetMyCustomProductsQuery { OwnerUserId = userAccessor.GetUserId() });

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CustomProductDto), 200)]
    public async Task<ActionResult> Details(string id)
    {
        var roles = await userAccessor.GetUserRolesAsync();
        return await HandleQuery(new GetCustomProductDetailsQuery
        {
            Id = id,
            RequesterUserId = userAccessor.GetUserId(),
            RequesterIsAdmin = roles.Contains(Roles.Admin)
        });
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet("admin")]
    [ProducesResponseType(typeof(List<CustomProductDto>), 200)]
    public Task<ActionResult> Admin([FromQuery] string? status)
        => HandleQuery(new GetCustomProductsAdminQuery { Status = status });

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{id}/admin/proposals")]
    [ProducesResponseType(typeof(CustomProductDto), 200)]
    public Task<ActionResult> AdminPropose(string id, [FromBody] ProposeCustomPriceDto dto)
        => HandleCommand(new AdminProposePriceCommand { CustomProductId = id, ProposeCustomPrice = dto });

    [HttpPost("{id}/proposals")]
    [ProducesResponseType(typeof(CustomProductDto), 200)]
    public Task<ActionResult> CustomerPropose(string id, [FromBody] ProposeCustomPriceDto dto)
        => HandleCommand(new CustomerProposePriceCommand
        {
            CustomProductId = id,
            OwnerUserId = userAccessor.GetUserId(),
            BasketId = basketProvider.GetBasketId(),
            ProposeCustomPrice = dto
        });

    [HttpPost("{id}/approve")]
    [ProducesResponseType(typeof(CustomProductDto), 200)]
    public async Task<ActionResult> Approve(string id)
    {
        var roles = await userAccessor.GetUserRolesAsync();
        var approver = roles.Contains(Roles.Admin) ? ProposalSource.Admin : ProposalSource.Customer;
        var basketId = approver == ProposalSource.Customer ? basketProvider.GetBasketId() : null;

        return await HandleCommand(new ApproveCustomProductPriceCommand
        {
            CustomProductId = id,
            Approver = approver,
            OwnerUserId = userAccessor.GetUserId(),
            BasketId = basketId
        });
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("{id}/reject")]
    [ProducesResponseType(200)]
    public Task<ActionResult> Reject(string id, [FromBody] string reason)
        => HandleCommand(new RejectCustomProductCommand { CustomProductId = id, Reason = reason });
}