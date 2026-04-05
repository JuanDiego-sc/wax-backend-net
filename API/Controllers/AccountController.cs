using Application.User.DTOs;
using Domain.Entities;
using Domain.Enumerators;
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

        await signInManager.UserManager.AddToRoleAsync(user, Roles.Member);

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

    //[Authorize]
    [HttpPost("address")]
    public async Task<ActionResult<Address>> CreateOrUpdateAddress(Address address)
    {
        var user = await signInManager.UserManager.Users
            .Include(x => x.Address)
            .FirstOrDefaultAsync(x => x.UserName == User.Identity!.Name);

        if( user == null) return Unauthorized();

        user.Address = address;

        var result = await signInManager.UserManager.UpdateAsync(user);

        if(!result.Succeeded) return BadRequest("Problem updating user address");
        return Ok(user.Address);

    }

    //[Authorize]
    [HttpGet("address")]
    public async Task<ActionResult<Address>> GetSavedAddress()
    {
        var address = await signInManager.UserManager.Users
            .Where(x => x.UserName == User.Identity!.Name)
            .Select(x => x.Address)
            .FirstOrDefaultAsync();

        if(address == null) return NoContent();

        return address;
    }
    
    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword(ForgotPasswordDto forgotPasswordDto)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(forgotPasswordDto.Email);
        
        if (user == null || !await signInManager.UserManager.IsEmailConfirmedAsync(user))
            return Ok();

        var code = await signInManager.UserManager.GeneratePasswordResetTokenAsync(user);
        
        await emailSender.SendPasswordResetCodeAsync(user, forgotPasswordDto.Email, code);

        return Ok();
    }
    
    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword(ResetPasswordDto passwordDto)
    {
        var user = await signInManager.UserManager.FindByEmailAsync(passwordDto.Email);

        if (user == null)
            return Ok();

        var result = await signInManager.UserManager.ResetPasswordAsync(
            user,
            passwordDto.Code,
            passwordDto.NewPassword);

        if (result.Succeeded) return Ok();
        foreach (var error in result.Errors)
            ModelState.AddModelError(error.Code, error.Description);
        return ValidationProblem();

    }
}
