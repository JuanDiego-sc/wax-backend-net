using Application.User.DTOs;
using Domain.Entities;
using Domain.Enumerators;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AdminController(UserManager<User> userManager) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await userManager.Users.ToListAsync();

        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new UserDto
            {
                Id = user.Id,
                Email = user.Email!,
                UserName = user.UserName!,
                EmailConfirmed = user.EmailConfirmed,
                Role = roles.FirstOrDefault()
            });
        }

        return Ok(result);
    }

    [HttpPost("{userId}/roles{roleName}")]
    public async Task<ActionResult> AddRoleToUser(string userId, string roleName)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) NotFound("User not found");

        if (!Roles.All.Contains(roleName)) return BadRequest("Role does not exists");
        
        var result = await userManager.AddToRoleAsync(user!, roleName);
        if (result.Succeeded) return Ok();
        
        return BadRequest(result.Errors.Select(e => e.Description));
    }
    
    [HttpDelete("{userId}/roles/{roleName}")]
    public async Task<ActionResult> RemoveUserFromRole(string userId, string roleName)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return NotFound("Usuario no encontrado");

        var result = await userManager.RemoveFromRoleAsync(user, roleName);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok();
    }

    [HttpPost("{userId}/disable")]
    public async Task<ActionResult> DisableUser(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return NotFound("Usuario no encontrado");

        user.LockoutEnd = DateTimeOffset.MaxValue;
        await userManager.UpdateAsync(user);

        return Ok();
    }

    [HttpPost("{userId}/enable")]
    public async Task<ActionResult> EnableUser(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null) return NotFound("Usuario no encontrado");

        user.LockoutEnd = null;
        await userManager.UpdateAsync(user);

        return Ok();
    }
}