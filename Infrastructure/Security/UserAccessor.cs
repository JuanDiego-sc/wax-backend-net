using System;
using System.Security.Claims;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Persistence;

namespace Infrastructure.Security;

public class UserAccessor(IHttpContextAccessor httpContextAccessor, AppDbContext dbContext) : IUserAccessor
{
    public async Task<User> GetUserAsync()
    {
        return await dbContext.Users.FindAsync(GetUserId())
            ?? throw new Exception("User not found.");
    }

    public string GetUserId()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) 
            ?? throw new Exception("User ID not found.");
    }
    public string GetUserEmail()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email) 
            ?? throw new Exception("User email not found.");
    }
}
