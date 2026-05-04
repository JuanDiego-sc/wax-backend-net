using System.Security.Claims;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Infrastructure.Security;

public class UserAccessor(
    IHttpContextAccessor httpContextAccessor, 
    WriteDbContext dbContext,
    UserManager<User> userManager) : IUserAccessor
{
    public async Task<User?> GetUserAsync()
    {
        return await dbContext.Users
            .FindAsync(GetUserId()) ;
    }

    public async Task<User?> GetUserWithBillingAddressAsync()
    {
        var userId= GetUserId();

        return await dbContext.Users
            .Include(x => x.BillingAddress)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<IList<string>> GetUserRolesAsync()
    {
        var user= await GetUserAsync();
        if (user == null) return Array.Empty<string>();
        
        return await userManager.GetRolesAsync(user);

    }

    public string GetUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return userId ?? throw new UnauthorizedAccessException("User ID not found.");
    }

    public string GetUserEmail()
    {
        var email = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
        return email ?? throw new UnauthorizedAccessException("User email not found.");
    }
}
