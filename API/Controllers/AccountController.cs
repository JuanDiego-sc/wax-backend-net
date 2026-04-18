using Application.User.Commands;
using Application.User.DTOs;
using Application.User.Queries;
using Domain.Entities;
using Domain.Enumerators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(SignInManager<User> signInManager, IEmailSender<User> emailSender) 
    : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult> RegisterUser(RegisterDto registerDto)
    {
        var user = new User{UserName = registerDto.Email, Email = registerDto.Email};

        var result = await signInManager.UserManager.CreateAsync(user, registerDto.Password);

        if(!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return ValidationProblem();
        }

        await signInManager.UserManager.AddToRoleAsync(user, Roles.Enrolled);

        return Ok();

    }   

    //[Authorize]
    [HttpGet("user-info")]
    public async Task<ActionResult> GetUserInfo()
    {
        if(User.Identity?.IsAuthenticated == false) return NoContent();

        var user = await signInManager.UserManager.GetUserAsync(User); //get the user by the claims

        if(user == null) return Unauthorized();

        var roles = await signInManager.UserManager.GetRolesAsync(user);

        return Ok(new
        {
            user.Email,
            user.UserName,
            Roles = roles
        });
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await signInManager.SignOutAsync();

        return NoContent();
    }

    [Authorize]
    [HttpPost("billing-address")]
    public async Task<ActionResult> CreateOrUpdateBillingAddress( CreateOrUpdateBillingInfoRequest billingInfo)
    {
        return await HandleCommand(new CreateOrUpdateBillingAddressCommand { BillingInfo = billingInfo });
    }

    [Authorize]
    [HttpGet("billing-address")]
    public async Task<ActionResult<BillingAddress>> GetSavedAddress()
    {
        return await HandleQuery(new GetBillingAddressQuery());
    }
    
    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordRequest forgotPasswordRequest)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(forgotPasswordRequest.Email);
        
        if (user == null )
            return Ok();

        var code = await signInManager.UserManager.GeneratePasswordResetTokenAsync(user);
        
        await emailSender.SendPasswordResetCodeAsync(user, forgotPasswordRequest.Email, code);

        return Ok();
    }
    
    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword(ResetPasswordRequest passwordRequest)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(passwordRequest.Email);

        if (user == null)
            return Ok();

        var result = await signInManager.UserManager.ResetPasswordAsync(
            user,
            passwordRequest.Code,
            passwordRequest.NewPassword);

        if (result.Succeeded) return Ok();
        foreach (var error in result.Errors)
            ModelState.AddModelError(error.Code, error.Description);
        return ValidationProblem();

    }
}
