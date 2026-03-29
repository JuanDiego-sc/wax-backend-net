using System.Security.Claims;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Infrastructure.Security;

public class UserAccessor(IHttpContextAccessor httpContextAccessor, WriteDbContext dbContext) : IUserAccessor
{
    public async Task<User?> GetUserAsync()
    {
        return await dbContext.Users.FindAsync(GetUserId());
    }

    public string GetUserId()
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? throw new UnauthorizedAccessException("User ID not found.");
    }
    public string GetUserEmail()
    {
        return httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email) 
            ?? throw new UnauthorizedAccessException("User email not found.");
    }
}
